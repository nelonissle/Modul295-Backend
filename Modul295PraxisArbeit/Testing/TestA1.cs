using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Modul295PraxisArbeitOrder.Services;
using Modul295PraxisArbeitOrder.Models;
using MongoDB.Driver;
using Modul295PraxisArbeit.Controllers;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Modul295PraxisArbeitOrder.Tests
{
    [TestFixture]
    public class OrderServiceControllerTests
    {
        private Mock<IOrderService> _mockOrderService;
        private ServiceOrdersController _controller;

        [SetUp]
        public void Setup()
        {
            _mockOrderService = new Mock<IOrderService>();
            _controller = new ServiceOrdersController(_mockOrderService.Object, null, null);
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

            _mockOrderService.Setup(s => s.CreateOrderAsync(newOrder)).Returns(Task.CompletedTask);

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

            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("Modul295Db");
            var orderCollection = database.GetCollection<OrderService>("OrderServices");

            await orderCollection.DeleteManyAsync(FilterDefinition<OrderService>.Empty);

            await orderCollection.InsertOneAsync(user1);
            await orderCollection.InsertOneAsync(user2);

            var ordersBeforeDelete = await orderCollection.Find(FilterDefinition<OrderService>.Empty).ToListAsync();
            Assert.AreEqual(2, ordersBeforeDelete.Count);

            await orderCollection.DeleteOneAsync(o => o.Name == user1.Name);

            var ordersAfterDelete = await orderCollection.Find(FilterDefinition<OrderService>.Empty).ToListAsync();

            Assert.AreEqual(1, ordersAfterDelete.Count);
            Assert.AreEqual(user2.Name, ordersAfterDelete[0].Name);
        }
    }
}
