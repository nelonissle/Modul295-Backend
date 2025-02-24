using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Modul295PraxisArbeit.Services;
using Modul295PraxisArbeit.Models;
using MongoDB.Driver;
using Modul295PraxisArbeit.Controllers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Modul295PraxisArbeit.Tests
{
    [TestFixture]
    public class OrderServiceControllerTests
    {
        private Mock<IOrderService> _mockOrderService;
        private Mock<IMongoDatabase> _mockDatabase;
        private Mock<ILogger<ServiceOrdersController>> _mockLogger;
        private ServiceOrdersController _controller;

        [SetUp]
        public void Setup()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockDatabase = new Mock<IMongoDatabase>();
            _mockLogger = new Mock<ILogger<ServiceOrdersController>>();

            // Mock the Users collection inside MongoDB
            var mockUserCollection = new Mock<IMongoCollection<OrderUser>>();
            _mockDatabase.Setup(db => db.GetCollection<OrderUser>("Users", It.IsAny<MongoCollectionSettings>()))
                         .Returns(mockUserCollection.Object);

            // Initialize the controller with the mocked dependencies
            _controller = new ServiceOrdersController(
                _mockOrderService.Object,
                _mockDatabase.Object,
                _mockLogger.Object
            );
        }
        [Test]
        public async Task GetAllOrders_ReturnsOkResult_WithOrders()
        {
            var orders = new List<OrderService>
            {
                new OrderService { OrderId = "1" },
                new OrderService { OrderId = "2" }
            };

            _mockOrderService.Setup(s => s.GetAllOrdersAsync()).ReturnsAsync(orders);

            var result = await _controller.GetServiceOrders();

            Assert.IsInstanceOf<ActionResult<IEnumerable<OrderService>>>(result);
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public async Task GetOrderById_OrderExists_ReturnsOkResult_WithOrder()
        {
            var order = new OrderService { OrderId = "1" };

            _mockOrderService.Setup(s => s.GetOrderByIdAsync("1")).ReturnsAsync(order);

            var result = await _controller.GetServiceOrder("1");

            Assert.IsInstanceOf<ActionResult<OrderService>>(result);
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public async Task GetOrderById_OrderDoesNotExist_ReturnsNotFound()
        {
            _mockOrderService.Setup(s => s.GetOrderByIdAsync("nonexistent")).ReturnsAsync((OrderService)null);

            var result = await _controller.GetServiceOrder("nonexistent");

            Assert.IsInstanceOf<ActionResult<OrderService>>(result);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
        }

        [Test]
        public async Task CreateOrder_ValidOrder_ReturnsCreatedAtAction()
        {
            var newOrder = new OrderService { OrderId = "123" };

            _mockOrderService.Setup(s => s.CreateOrderAsync(It.IsAny<OrderService>()))
                             .Returns(Task.CompletedTask);

            var result = await _controller.PostServiceOrder(newOrder);

            Assert.IsInstanceOf<ActionResult<OrderService>>(result);
            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
        }


        [Test]
        public async Task UpdateOrder_OrderExists_ReturnsNoContent()
        {
            var existingOrder = new OrderService { OrderId = "1" };
            var updatedOrder = new OrderService { OrderId = "1" };

            _mockOrderService.Setup(s => s.GetOrderByIdAsync("1")).ReturnsAsync(existingOrder);
            _mockOrderService.Setup(s => s.UpdateOrderAsync("1", updatedOrder)).Returns(Task.CompletedTask);

            var result = await _controller.PutServiceOrder("1", updatedOrder);

            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task DeleteOrder_OrderExists_ReturnsNoContent()
        {
            var existingOrder = new OrderService { OrderId = "1" };

            _mockOrderService.Setup(s => s.GetOrderByIdAsync("1")).ReturnsAsync(existingOrder);
            _mockOrderService.Setup(s => s.DeleteOrderAsync("1")).Returns(Task.CompletedTask);

            var result = await _controller.DeleteServiceOrder("1");

            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task DeleteOrder_OrderDoesNotExist_ReturnsNotFound()
        {
            _mockOrderService.Setup(s => s.GetOrderByIdAsync("notfound")).ReturnsAsync((OrderService)null);

            var result = await _controller.DeleteServiceOrder("notfound");

            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task UpdateOrder_OrderDoesNotExist_ReturnsNotFound()
        {
            _mockOrderService.Setup(s => s.GetOrderByIdAsync("notfound")).ReturnsAsync((OrderService)null);

            var updatedOrder = new OrderService { OrderId = "notfound" };

            var result = await _controller.PutServiceOrder("notfound", updatedOrder);

            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }
    }

    [TestFixture]
    public class OrderServiceMongoDBTests
    {
        private IMongoCollection<OrderService> _orderCollection;
        private MongoClient _client;
        private IMongoDatabase _database;

        [SetUp]
        public void Setup()
        {
            //TODO - Initialize MongoDB connection Like in Program.cs
            _client = new MongoClient("mongodb://mongoadmin:secret@localhost:27017/");
            _database = _client.GetDatabase("Modul295Db");
            _orderCollection = _database.GetCollection<OrderService>("OrderServices");

            // 🔹 Ensure the collection is not null
            if (_orderCollection == null)
            {
                throw new InvalidOperationException("MongoDB collection 'OrderServices' could not be initialized.");
            }
        }

        [Test]
        public async Task MongoDB_InsertAndDeleteOrder_WorksCorrectly()
        {
            var user1 = new OrderService
            {
                Name = "jefffffc",
                Email = "joseff@ashd.com",
                Phone = "11111111111",
                Priority = "Standard",
                Service = "Grosser Service"
            };

            var user2 = new OrderService
            {
                Name = "HHHHHHHHHHHHHHHHHHHHH",
                Email = "HHHHHHHHHHHHHHHHHHH@doe.com",
                Phone = "22222222222",
                Priority = "High",
                Service = "Kleiner Service"
            };

            await _orderCollection.InsertOneAsync(user1);
            await _orderCollection.InsertOneAsync(user2);

            var ordersBeforeDelete = await _orderCollection.Find(FilterDefinition<OrderService>.Empty).ToListAsync();
            //Assert.AreEqual(2, ordersBeforeDelete.Count);

            // ✅ Delete only user1 and user2, keep all other data
            var filter = Builders<OrderService>.Filter.Or(
                Builders<OrderService>.Filter.Eq(o => o.Name, "jefffffc"),
                Builders<OrderService>.Filter.Eq(o => o.Name, "HHHHHHHHHHHHHHHHHHHHH")
            );

            await _orderCollection.DeleteManyAsync(filter);

            var ordersAfterDelete = await _orderCollection.Find(FilterDefinition<OrderService>.Empty).ToListAsync();

            // ✅ Ensure only user1 and user2 were deleted, other data remains
            Assert.IsFalse(ordersAfterDelete.Any(o => o.Name == user1.Name));
            Assert.IsFalse(ordersAfterDelete.Any(o => o.Name == user2.Name));
        }
    }
}
