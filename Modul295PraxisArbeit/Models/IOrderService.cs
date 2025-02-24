using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modul295PraxisArbeit.Models
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
