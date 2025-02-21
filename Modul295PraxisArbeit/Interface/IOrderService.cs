using System.Collections.Generic;
using System.Threading.Tasks;
using Modul295PraxisArbeit.Models;

namespace Modul295PraxisArbeit.Interfaces
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
