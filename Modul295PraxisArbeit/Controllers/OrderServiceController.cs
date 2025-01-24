using Microsoft.AspNetCore.Mvc;
using Modul295PraxisArbeitOrder.Models;
using Modul295PraxisArbeitOrder.Services;

namespace Modul295PraxisArbeitOrder.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderServiceController : ControllerBase
    {
        private readonly OrderServiceService _orderService;

        public OrderServiceController(OrderServiceService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(string id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(OrderService newOrder)
        {
            await _orderService.CreateOrderAsync(newOrder);
            return CreatedAtAction(nameof(GetOrderById), new { id = newOrder.OrderId }, newOrder);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(string id, OrderService updatedOrder)
        {
            var existingOrder = await _orderService.GetOrderByIdAsync(id);
            if (existingOrder == null) return NotFound();

            updatedOrder.OrderId = id; // Ensure the ID is preserved
            await _orderService.UpdateOrderAsync(id, updatedOrder);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            var existingOrder = await _orderService.GetOrderByIdAsync(id);
            if (existingOrder == null) return NotFound();

            await _orderService.DeleteOrderAsync(id);
            return NoContent();
        }
    }
}
