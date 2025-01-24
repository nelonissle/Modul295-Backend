using Modul295PraxisArbeitOrder.Data;
using Modul295PraxisArbeitOrder.Models;
using Modul295PraxisArbeitOrder.Services;

class Test
{
    static async Task Main(string[] args)
    {
        var dbContext = new MongoDbContext("mongodb://localhost:27017", "Modul295Db");
        var orderService = new OrderServiceService(dbContext);

        // Create a new order
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

        // Retrieve all orders
        var orders = await orderService.GetAllOrdersAsync();
        Console.WriteLine("All Orders:");
        foreach (var order in orders)
        {
            Console.WriteLine($"Order ID: {order.OrderId}, Name: {order.Name}, Status: {order.Status}");
        }

        // Update an order
        if (orders.Count > 0)
        {
            var firstOrder = orders[0];
            firstOrder.Status = "Completed";
            await orderService.UpdateOrderAsync(firstOrder.OrderId!, firstOrder);
            Console.WriteLine($"Order {firstOrder.OrderId} updated to Completed.");
        }

        // Delete an order
        if (orders.Count > 0)
        {
            var firstOrder = orders[0];
            await orderService.DeleteOrderAsync(firstOrder.OrderId!);
            Console.WriteLine($"Order {firstOrder.OrderId} deleted.");
        }
    }
}
