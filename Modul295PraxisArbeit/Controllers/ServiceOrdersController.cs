using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Modul295PraxisArbeitOrder.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Modul295PraxisArbeitOrder.Services;
using Modul295PraxisArbeit.Models;

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
            _orderService = orderService;
            _usersCollection = database.GetCollection<OrderUser>("Users");
            _logger = logger;
        }

        public bool CheckEditRole(string username)
        {
            _logger.LogInformation($"Check Edit Role of User: {username}");
            var user = _usersCollection.Find(u => u.Username == username).SingleOrDefault();
            return user != null;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderService>>> GetServiceOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderService>> GetServiceOrder(string id)
        {
            var serviceOrder = await _orderService.GetOrderByIdAsync(id);
            if (serviceOrder == null)
            {
                return NotFound();
            }
            return Ok(serviceOrder);
        }

        [Authorize]
        [HttpGet("User/{id}")]
        public async Task<ActionResult<IEnumerable<OrderService>>> GetServiceOrderByUser(string id)
        {
            var serviceOrders = await _orderService.GetAllOrdersAsync();
            var userOrders = serviceOrders.Where(s => s.AssignedUserId == id).ToList();
            return Ok(userOrders);
        }

        
        [HttpPost]
        public async Task<ActionResult<OrderService>> PostServiceOrder(OrderService serviceOrder)
        {
            if (string.IsNullOrEmpty(serviceOrder.Status))
                serviceOrder.Status = "Offen";

            var userName = HttpContext.User.Identity.Name;
            var user = await _usersCollection.Find(u => u.Username == userName).FirstOrDefaultAsync();
            if (user != null)
                serviceOrder.AssignedUserId = user.Id;

            await _orderService.CreateOrderAsync(serviceOrder);
            return CreatedAtAction(nameof(GetServiceOrder), new { id = serviceOrder.OrderId }, serviceOrder);
        }


        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceOrder(string id, OrderService serviceOrder)
        {
            var existingOrder = await _orderService.GetOrderByIdAsync(id);
            if (existingOrder == null) return NotFound();

            serviceOrder.OrderId = id;
            await _orderService.UpdateOrderAsync(id, serviceOrder);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceOrder(string id)
        {
            var existingOrder = await _orderService.GetOrderByIdAsync(id);
            if (existingOrder == null) return NotFound();

            await _orderService.DeleteOrderAsync(id);
            return NoContent();
        }
    }
}
