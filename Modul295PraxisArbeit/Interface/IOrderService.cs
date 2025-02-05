using System.Collections.Generic;
using System.Threading.Tasks;
using Modul295PraxisArbeitOrder.Models;

namespace Modul295PraxisArbeitOrder.Services
{
    public interface IOrderService
    {
        Task<List<OrderService>> GetAllOrdersAsync();
        Task<OrderService> GetOrderByIdAsync(string id);
        Task CreateOrderAsync(OrderService newOrder);
        Task UpdateOrderAsync(string id, OrderService updatedOrder);
        Task DeleteOrderAsync(string id);
    }
}
