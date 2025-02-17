using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Modul295PraxisArbeitOrder.Models;
using Modul295PraxisArbeitOrder.Services;

class Test
{
    static async Task Main(string[] args)
    {
        // 1. Verbindung zur MongoDB herstellen
        var client = new MongoClient("mongodb://localhost:27017");
        var database = client.GetDatabase("Modul295Db");
        var orderService = new OrderServiceService(database);

        // 2. Neue Bestellung erstellen
        var newOrder = new OrderService
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "123456789",
            Priority = "High",
            Service = "Repair",
            Status = "Open"
        };

        await orderService.CreateOrderAsync(newOrder);
        Console.WriteLine("New order created.");

        // 3. Alle Bestellungen abrufen
        var orders = await orderService.GetAllOrdersAsync();
        Console.WriteLine("All Orders:");
        foreach (var order in orders)
        {
            Console.WriteLine($"Order ID: {order.OrderId}, Name: {order.Name}, Status: {order.Status}");
        }

        // 4. Erste Bestellung aktualisieren (wenn vorhanden)
        if (orders.Count > 0)
        {
            var firstOrder = orders[0];

            if (!string.IsNullOrEmpty(firstOrder.OrderId))
            {
                firstOrder.Status = "Completed";
                await orderService.UpdateOrderAsync(firstOrder.OrderId, firstOrder);
                Console.WriteLine($"Order {firstOrder.OrderId} updated to Completed.");
            }
            else
            {
                Console.WriteLine("Error: Order ID is null.");
            }
        }

        // 5. Erste Bestellung löschen (wenn vorhanden)
        if (orders.Count > 0)
        {
            var firstOrder = orders[0];

            if (!string.IsNullOrEmpty(firstOrder.OrderId))
            {
                await orderService.DeleteOrderAsync(firstOrder.OrderId);
                Console.WriteLine($"Order {firstOrder.OrderId} deleted.");
            }
            else
            {
                Console.WriteLine("Error: Order ID is null.");
            }
        }
    }
}
