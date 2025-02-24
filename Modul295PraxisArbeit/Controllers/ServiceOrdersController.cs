using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Modul295PraxisArbeit.Models;
using Modul295PraxisArbeit.Services;
using Modul295PraxisArbeit.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Modul295PraxisArbeit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IMongoCollection<OrderUser> _usersCollection;
        private readonly ILogger<ServiceOrdersController> _logger;

        public ServiceOrdersController(IOrderService orderService, IMongoDatabase database, ILogger<ServiceOrdersController> logger)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _usersCollection = database?.GetCollection<OrderUser>("Users") ?? throw new ArgumentNullException(nameof(database));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Checks if the user has editing permissions.
        /// </summary>
        public bool CheckEditRole(string username)
        {
            _logger.LogInformation($"Checking Edit Role for User: {username}");

            if (string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("Username is null or empty.");
                return false;
            }

            var user = _usersCollection.Find(u => u.Username == username).SingleOrDefault();
            return user != null;
        }

        /// <summary>
        /// Retrieves all service orders.
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderService>>> GetServiceOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        /// <summary>
        /// Retrieves a specific service order by ID.
        /// </summary>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderService>> GetServiceOrder(string id)
        {
            var serviceOrder = await _orderService.GetOrderByIdAsync(id);
            if (serviceOrder == null)
            {
                _logger.LogWarning($"Service order with ID {id} not found.");
                return NotFound();
            }
            return Ok(serviceOrder);
        }

        /// <summary>
        /// Retrieves all service orders assigned to a specific user.
        /// </summary>
        [Authorize]
        [HttpGet("User/{id}")]
        public async Task<ActionResult<IEnumerable<OrderService>>> GetServiceOrderByUser(string id)
        {
            var serviceOrders = await _orderService.GetAllOrdersAsync();
            var userOrders = serviceOrders.Where(s => s.AssignedUserId == id).ToList();

            if (!userOrders.Any())
            {
                _logger.LogWarning($"No service orders found for user ID {id}.");
            }

            return Ok(userOrders);
        }

        /// <summary>
        /// Creates a new service order.
        /// </summary>
        //[Authorize]
        [HttpPost]
        public async Task<ActionResult<OrderService>> PostServiceOrder(OrderService serviceOrder)
        {
            if (serviceOrder == null)
            {
                _logger.LogWarning("Received null order data.");
                return BadRequest("Invalid order data.");
            }

            if (string.IsNullOrEmpty(serviceOrder.Status))
                serviceOrder.Status = "Offen";

            try
            {
                string userName = HttpContext?.User?.Identity?.Name;
                if (!string.IsNullOrEmpty(userName))
                {
                    var user = await _usersCollection.Find(u => u.Username == userName).FirstOrDefaultAsync();
                    if (user != null)
                        serviceOrder.AssignedUserId = user.Id;
                }

                await _orderService.CreateOrderAsync(serviceOrder);
                _logger.LogInformation($"Order created successfully with ID {serviceOrder.OrderId}.");

                return CreatedAtAction(nameof(GetServiceOrder), new { id = serviceOrder.OrderId }, serviceOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while creating service order: {ex.Message}");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Updates an existing service order.
        /// </summary>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceOrder(string id, OrderService serviceOrder)
        {
            if (serviceOrder == null)
            {
                _logger.LogWarning($"Attempted to update order {id} with null data.");
                return BadRequest("Invalid order data.");
            }

            var existingOrder = await _orderService.GetOrderByIdAsync(id);
            if (existingOrder == null)
            {
                _logger.LogWarning($"Service order with ID {id} not found.");
                return NotFound();
            }

            serviceOrder.OrderId = id;
            await _orderService.UpdateOrderAsync(id, serviceOrder);
            _logger.LogInformation($"Order with ID {id} updated successfully.");
            return NoContent();
        }

        /// <summary>
        /// Deletes an existing service order.
        /// </summary>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceOrder(string id)
        {
            var existingOrder = await _orderService.GetOrderByIdAsync(id);
            if (existingOrder == null)
            {
                _logger.LogWarning($"Attempted to delete non-existent order with ID {id}.");
                return NotFound();
            }

            await _orderService.DeleteOrderAsync(id);
            _logger.LogInformation($"Order with ID {id} deleted successfully.");
            return NoContent();
        }
    }
}
