using MongoDB.Driver;
using Modul295PraxisArbeit.Models;

namespace Modul295PraxisArbeit.Services
{
    public class OrderServiceService : IOrderService
    {
        private readonly IMongoCollection<OrderService> _orderCollection;

        public OrderServiceService(IMongoDatabase database)
        {
            _orderCollection = database.GetCollection<OrderService>("OrderServices");
        }

        public async Task<List<OrderService>> GetAllOrdersAsync()
        {
            return await _orderCollection.Find(_ => true).ToListAsync();
        }

        public async Task<OrderService?> GetOrderByIdAsync(string id)
        {
            return await _orderCollection.Find(order => order.OrderId == id).FirstOrDefaultAsync();
        }

        public async Task CreateOrderAsync(OrderService newOrder)
        {
            await _orderCollection.InsertOneAsync(newOrder);
        }

        public async Task UpdateOrderAsync(string id, OrderService updatedOrder)
        {
            await _orderCollection.ReplaceOneAsync(order => order.OrderId == id, updatedOrder);
        }

        public async Task DeleteOrderAsync(string id)
        {
            await _orderCollection.DeleteOneAsync(order => order.OrderId == id);
        }
    }
}
