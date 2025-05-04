using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;

class Product
{
    public string Name { get; set; }
    public double Price { get; set; }
    public int Stock { get; set; }

    public Product(string name, double price, int stock)
    {
        Name = name;
        Price = price;
        Stock = stock;
    }
}

class Inventory
{
    public List<Product> Products { get; private set; } = new List<Product>();

    public Inventory()
    {
        Products.Add(new Product("T-shirt", 15.0, 10));
        Products.Add(new Product("Hat", 10.0, 5));
        Products.Add(new Product("Shoes", 50.0, 3));
    }

    public string GetProductList()
    {
        return JsonSerializer.Serialize(Products);
    }

    public bool ProcessOrder(string productName, int quantity)
    {
        lock (Products)
        {
            var product = Products.Find(p => p.Name.ToLower() == productName.ToLower());
            if (product != null && product.Stock >= quantity)
            {
                product.Stock -= quantity;
                return true;
            }
            return false;
        }
    }
}

class Program
{
    static Inventory inventory = new Inventory();

    static void HandleClient(TcpClient client)
    {
        var stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int byteCount;

        string productsJson = inventory.GetProductList();
        byte[] productBytes = Encoding.UTF8.GetBytes(productsJson);
        stream.Write(productBytes, 0, productBytes.Length);

        byteCount = stream.Read(buffer, 0, buffer.Length);
        string order = Encoding.UTF8.GetString(buffer, 0, byteCount);
        string[] parts = order.Split(',');

        string item = parts[0];
        int qty = int.Parse(parts[1]);

        bool success = inventory.ProcessOrder(item, qty);

        string response = success ? "Order confirmed!" : "Order failed: insufficient stock.";
        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        stream.Write(responseBytes, 0, responseBytes.Length);

        client.Close();
    }

    static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 5000);
        server.Start();
        Console.WriteLine("Shop server started...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }
}
