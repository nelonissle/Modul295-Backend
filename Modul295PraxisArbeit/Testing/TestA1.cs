using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Modul295PraxisArbeitOrder.Controllers;
using Modul295PraxisArbeitOrder.Services;
using Modul295PraxisArbeitOrder.Models;
using Modul295PraxisArbeitOrder.Data;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;



namespace Modul295PraxisArbeitOrder.Tests
{
    [TestFixture]
    public class OrderServiceControllerTests
    {
        private Mock<IOrderService> _mockOrderService;
        private OrderServiceController _controller;

        [SetUp]
        public void Setup()
        {
            _mockOrderService = new Mock<IOrderService>();
            _controller = new OrderServiceController(_mockOrderService.Object);
        }

        [Test]
        public async Task GetAllOrders_ReturnsOkResult_WithOrders()
        {
            var orders = new List<OrderService> // Corrected class name
            {
                new OrderService { OrderId = "1" },
                new OrderService { OrderId = "2" }
            };

            _mockOrderService.Setup(s => s.GetAllOrdersAsync()).ReturnsAsync(orders);

            var result = await _controller.GetAllOrders();

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(orders, okResult.Value);
        }

        [Test]
        public async Task GetOrderById_OrderExists_ReturnsOkResult_WithOrder()
        {
            var order = new OrderService { OrderId = "1" };

            _mockOrderService.Setup(s => s.GetOrderByIdAsync("1")).ReturnsAsync(order);

            var result = await _controller.GetOrderById("1");

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(order, okResult.Value);
        }

        [Test]
        public async Task GetOrderById_OrderDoesNotExist_ReturnsNotFound()
        {
            _mockOrderService.Setup(s => s.GetOrderByIdAsync("nonexistent")).ReturnsAsync((OrderService)null);

            var result = await _controller.GetOrderById("nonexistent");

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task CreateOrder_ValidOrder_ReturnsCreatedAtAction()
        {
            var newOrder = new OrderService { OrderId = "123" };

            _mockOrderService.Setup(s => s.CreateOrderAsync(newOrder)).Returns(Task.CompletedTask);

            var result = await _controller.CreateOrder(newOrder);

            Assert.IsInstanceOf<CreatedAtActionResult>(result);
            var createdResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(nameof(OrderServiceController.GetOrderById), createdResult.ActionName);
            Assert.AreEqual(newOrder.OrderId, createdResult.RouteValues["id"]);
            Assert.AreEqual(newOrder, createdResult.Value);
        }

        [Test]
        public async Task UpdateOrder_OrderDoesNotExist_ReturnsNotFound()
        {
            _mockOrderService.Setup(s => s.GetOrderByIdAsync("nonexistent")).ReturnsAsync((OrderService)null);

            var updatedOrder = new OrderService { OrderId = "nonexistent" };

            var result = await _controller.UpdateOrder("nonexistent", updatedOrder);

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task UpdateOrder_OrderExists_ReturnsNoContent()
        {
            var existingOrder = new OrderService { OrderId = "1" };
            var updatedOrder = new OrderService { OrderId = "1" };

            _mockOrderService.Setup(s => s.GetOrderByIdAsync("1")).ReturnsAsync(existingOrder);
            _mockOrderService.Setup(s => s.UpdateOrderAsync("1", updatedOrder)).Returns(Task.CompletedTask);

            var result = await _controller.UpdateOrder("1", updatedOrder);

            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task DeleteOrder_OrderDoesNotExist_ReturnsNotFound()
        {
            _mockOrderService.Setup(s => s.GetOrderByIdAsync("nonexistent")).ReturnsAsync((OrderService)null);

            var result = await _controller.DeleteOrder("nonexistent");

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task DeleteOrder_OrderExists_ReturnsNoContent()
        {
            var existingOrder = new OrderService { OrderId = "1" };

            _mockOrderService.Setup(s => s.GetOrderByIdAsync("1")).ReturnsAsync(existingOrder);
            _mockOrderService.Setup(s => s.DeleteOrderAsync("1")).Returns(Task.CompletedTask);

            var result = await _controller.DeleteOrder("1");

            Assert.IsInstanceOf<NoContentResult>(result);
        }


        [Test]
        public async Task TestUpdate_OrderExists_ReturnsNoContent()
        {
            // Arrange: Erstelle zwei Bestellobjekte
            var user1 = new OrderService
            {
                Name = "jefffffc",
                Email = "joseff@ashd.com",
                Phone = "11111111111",
                Priority = "Standard",
                Service = "Grosser Service",
                Status = null,
                AssignedUserId = null,
                AssignedUser = null
            };

            var user2 = new OrderService
            {
                Name = "HHHHHHHHHHHHHHHHHHHHH",
                Email = "HHHHHHHHHHHHHHHHHHH@doe.com",
                Phone = "22222222222",
                Priority = "High",
                Service = "Kleiner Service",
                Status = null,
                AssignedUserId = null,
                AssignedUser = null
            };

            // MongoDB-Setup
            var client = new MongoClient("mongodb://localhost:27017"); // MongoDB-Verbindung (stelle sicher, dass MongoDB läuft)
            var database = client.GetDatabase("Modul295Db");
            var orderCollection = database.GetCollection<OrderService>("OrderServices");

            // Löschen vorhandener Daten in der Sammlung, falls nötig
           await orderCollection.DeleteManyAsync(FilterDefinition<OrderService>.Empty);

            // Act: Füge die Bestellungen hinzu
            await orderCollection.InsertOneAsync(user1); // Benutzer 1 wird hinzugefügt
            await orderCollection.InsertOneAsync(user2); // Benutzer 2 wird hinzugefügt

            // Überprüfen, dass beide Bestellungen in der Datenbank sind
            var ordersBeforeDelete = await orderCollection.Find(FilterDefinition<OrderService>.Empty).ToListAsync();
            Assert.AreEqual(2, ordersBeforeDelete.Count); // Es sollten 2 Bestellungen in der DB sein

            // Lösche Benutzer 1 basierend auf dem Namen
            await orderCollection.DeleteOneAsync(o => o.Name == user1.Name);

            // Act: Liste der Bestellungen nach Löschung abrufen
            var ordersAfterDelete = await orderCollection.Find(FilterDefinition<OrderService>.Empty).ToListAsync();

            // Assert: Überprüfen, dass Benutzer 1 aus der Liste gelöscht wurde und nur Benutzer 2 übrig bleibt
            Assert.AreEqual(1, ordersAfterDelete.Count);  // Es sollte nur noch 1 Bestellung (user2) in der DB sein
            Assert.AreEqual(user2.Name, ordersAfterDelete[0].Name); // Nur Benutzer 2 sollte in der Liste der Bestellungen sein
        }
    }
}
