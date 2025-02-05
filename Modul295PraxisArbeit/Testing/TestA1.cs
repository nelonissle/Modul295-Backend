using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Modul295PraxisArbeitOrder.Controllers;
using Modul295PraxisArbeitOrder.Services;
using Modul295PraxisArbeitOrder.Models;


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
    }
}
