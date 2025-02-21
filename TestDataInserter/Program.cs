using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Modul295PraxisArbeit.Models;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("📌 Inserting Test Data into MongoDB...");

        var client = new MongoClient("mongodb://localhost:27017");
        var database = client.GetDatabase("Modul295Db");
        var collection = database.GetCollection<OrderService>("OrderServices");

        var testOrders = new List<OrderService>
        {
            new OrderService { Name = "John Doe", Email = "john.doe@example.com", Phone = "123456789", Priority = "High", Service = "Full Ski Service", Status = "Offen" },
            new OrderService { Name = "Jane Smith", Email = "jane.smith@example.com", Phone = "987654321", Priority = "Medium", Service = "Edge Tuning", Status = "InArbeit" },
            new OrderService { Name = "Mark Miller", Email = "mark.miller@example.com", Phone = "741852963", Priority = "Low", Service = "Waxing", Status = "Abgeschlossen" }
        };

        await collection.InsertManyAsync(testOrders);
        Console.WriteLine("✅ Inserted test data successfully!");
    }
}
