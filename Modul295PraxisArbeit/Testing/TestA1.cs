using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Modul295PraxisArbeitOrder.Controllers;
using Modul295PraxisArbeitOrder.Services;
using Modul295PraxisArbeitOrder.Models;
using Modul295PraxisArbeitOrder.Data;



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
        
        /*
        [Test]
        public async Task TestUpdate_OrderExists_ReturnsNoContennt()
        {
            // Arrange: Erstelle zwei Benutzerobjekte
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
                Name = "john_doe",
                Email = "john@doe.com",
                Phone = "22222222222",
                Priority = "High",
                Service = "Kleiner Service",
                Status = null,
                AssignedUserId = null,
                AssignedUser = null
            };

            // Mock-Setup für die Services
            _mockOrderService.Setup(s => s.CreateOrderAsync(user1)).Returns(Task.CompletedTask);
            _mockOrderService.Setup(s => s.CreateOrderAsync(user2)).Returns(Task.CompletedTask);
            _mockOrderService.Setup(s => s.DeleteOrderAsync(user1.Name)).Returns(Task.CompletedTask);

            // Benutzer 1 und Benutzer 2 hinzufügen
            await _controller.AddOrderService(user1); // Benutzer 1 wird hinzugefügt
            await _controller.AddOrderService(user2); // Benutzer 2 wird hinzugefügt

            // Benutzer 1 löschen
            await _controller.DeleteUser(user1.Id); // Benutzer 1 wird gelöscht

            // Act: Liste der Benutzer abrufen
            _mockOrderService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(new List<User> { user2 });

            var result = await _controller.GetAllUsers();

            // Assert: Überprüfen, dass der gelöschte Benutzer nicht mehr in der Liste ist
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var usersInList = okResult.Value as List<User>;
            Assert.IsNotNull(usersInList);
            Assert.AreEqual(1, usersInList.Count);
            Assert.AreEqual(user2.Id, usersInList[0].Id); // Nur Benutzer 2 sollte in der Liste sein
        }
*/
    }
}
