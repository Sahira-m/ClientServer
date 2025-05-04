using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

class Product
{
    public string Name { get; set; }
    public double Price { get; set; }
    public int Stock { get; set; }
}

class Program
{
    static void Main()
    {
        TcpClient client = new TcpClient("127.0.0.1", 5000);
        var stream = client.GetStream();
        byte[] buffer = new byte[2048];
        int byteCount = stream.Read(buffer, 0, buffer.Length);

        string productsJson = Encoding.UTF8.GetString(buffer, 0, byteCount);
        var products = JsonSerializer.Deserialize<List<Product>>(productsJson);

        Console.WriteLine("Available Products:");
        foreach (var p in products)
        {
            Console.WriteLine($"{p.Name} - ${p.Price} ({p.Stock} in stock)");
        }

        Console.Write("\nEnter product to buy: ");
        string item = Console.ReadLine();

        Console.Write("Enter quantity: ");
        int qty = int.Parse(Console.ReadLine());

        string order = $"{item},{qty}";
        byte[] orderBytes = Encoding.UTF8.GetBytes(order);
        stream.Write(orderBytes, 0, orderBytes.Length);

        byteCount = stream.Read(buffer, 0, buffer.Length);
        string response = Encoding.UTF8.GetString(buffer, 0, byteCount);

        Console.WriteLine("\nServer Response: " + response);
        client.Close();
    }
}
