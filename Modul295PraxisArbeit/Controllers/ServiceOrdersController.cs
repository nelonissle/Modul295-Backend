using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Modul295PraxisArbeitOrder.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Modul295PraxisArbeit.Models;

namespace Modul295PraxisArbeit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceOrdersController : ControllerBase
    {
        private readonly IMongoCollection<OrderService> _serviceOrdersCollection;
        private readonly IMongoCollection<OrderUser> _usersCollection;
        private readonly ILogger<ServiceOrdersController> _logger;

        public ServiceOrdersController(IMongoDatabase database, ILogger<ServiceOrdersController> logger)
        {
            _serviceOrdersCollection = database.GetCollection<OrderService>("ServiceOrders");
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
            var orders = await _serviceOrdersCollection.Find(_ => true).ToListAsync();
            return Ok(orders);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderService>> GetServiceOrder(string id)
        {
            var serviceOrder = await _serviceOrdersCollection.Find(s => s.OrderId == id).FirstOrDefaultAsync();
            if (serviceOrder == null)
            {
                return NotFound();
            }
            return serviceOrder;
        }

        [Authorize]
        [HttpGet("User/{id}")]
        public async Task<ActionResult<IEnumerable<OrderService>>> GetServiceOrderByUser(string id)
        {
            var serviceOrders = await _serviceOrdersCollection.Find(s => s.AssignedUserId == id).ToListAsync();
            return Ok(serviceOrders);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<OrderService>> PostServiceOrder(OrderService serviceOrder)
        {
            if (string.IsNullOrEmpty(serviceOrder.Status))
                serviceOrder.Status = "Offen";

            var userName = HttpContext.User.Identity.Name;
            var user = _usersCollection.Find(u => u.Username == userName).FirstOrDefault();
            if (user != null)
                serviceOrder.AssignedUserId = user.Id;

            await _serviceOrdersCollection.InsertOneAsync(serviceOrder);
            return CreatedAtAction(nameof(GetServiceOrder), new { id = serviceOrder.OrderId }, serviceOrder);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceOrder(string id, OrderService serviceOrder)
        {
            var update = Builders<OrderService>.Update
                .Set(s => s.Name, serviceOrder.Name)
                .Set(s => s.Email, serviceOrder.Email)
                .Set(s => s.Phone, serviceOrder.Phone)
                .Set(s => s.Priority, serviceOrder.Priority)
                .Set(s => s.Service, serviceOrder.Service)
                .Set(s => s.Status, serviceOrder.Status);

            await _serviceOrdersCollection.UpdateOneAsync(s => s.OrderId == id, update);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceOrder(string id)
        {
            await _serviceOrdersCollection.DeleteOneAsync(s => s.OrderId == id);
            return NoContent();
        }
    }
}
