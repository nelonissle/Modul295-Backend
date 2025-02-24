using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Modul295PraxisArbeit.Models;
using BCrypt.Net;  // Make sure to add the BCrypt.Net NuGet package

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("📌 Inserting Test Data into MongoDB...");

        var client = new MongoClient("mongodb://mongoadmin:secret@mymongo:27017");
        var database = client.GetDatabase("Modul295Db");

        database.CreateCollection("OrderServices");
        database.CreateCollection("Users");

        var collection = database.GetCollection<OrderService>("OrderServices");
        var usercol = database.GetCollection<OrderUser>("Users");

        var testOrders = new List<OrderService>
        {
            new OrderService { Name = "John Doe", Email = "john.doe@example.com", Phone = "123456789", Priority = "High", Service = "Full Ski Service", Status = "Offen" },
            new OrderService { Name = "Jane Smith", Email = "jane.smith@example.com", Phone = "987654321", Priority = "Medium", Service = "Edge Tuning", Status = "InArbeit" },
            new OrderService { Name = "Mark Miller", Email = "mark.miller@example.com", Phone = "741852963", Priority = "Low", Service = "Waxing", Status = "Abgeschlossen" }
        };
        await collection.InsertManyAsync(testOrders);

        // Create password hash for the admin user using the CreatePasswordHash function
        string adminPasswordHash = CreatePasswordHash("1234");

        var testUsers = new List<OrderUser>
        {
            new OrderUser
            {
                Username = "nelo@admin.com",
                PasswordHash = adminPasswordHash, // Hashed password is stored here
                Role = "Admin",
                TwoFactorCode = null,
                TwoFactorEnabled = false,
                TwoFactorSecret = null,
                TwoFactorRecoveryCodes = null,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new OrderUser
            {
                Username = "testuser",
                PasswordHash = "hashedpassword2",
                Role = "user",
                TwoFactorEnabled = false,
                TwoFactorSecret = null,
                TwoFactorRecoveryCodes = null
            }
        };

        await usercol.InsertManyAsync(testUsers);

        Console.WriteLine("✅ Inserted test data successfully!");
    }

    // Function to hash a password using BCrypt
    private static string CreatePasswordHash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
