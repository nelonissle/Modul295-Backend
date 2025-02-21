using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;
using Modul295PraxisArbeit.Models;
using Modul295PraxisArbeit.Data;
using Modul295PraxisArbeit.Interfaces;

namespace Modul295PraxisArbeit.Services
{
    public class OrderServiceService : IOrderService
    {
        // private readonly IMongoCollection<OrderService> _orderCollection;

        //public OrderServiceService(IMongoDatabase database)
        //{
        //    _orderCollection = database.GetCollection<OrderService>("OrderServices");
        //}

        private readonly ApplicationDbContext _dbContext;

        public OrderServiceService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<List<OrderService>> GetAllOrdersAsync()
        {
            return await _dbContext.OrderServices.ToListAsync();
        }

        public async Task<OrderService?> GetOrderByIdAsync(string id)
        {
            // get first order from dbContext where order id matches the id
            return await _dbContext.OrderServices.FirstOrDefaultAsync(p => p.OrderId == id);
        }

        public async Task CreateOrderAsync(OrderService newOrder)
        {

            _dbContext.OrderServices.Add(newOrder);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(string id, OrderService updatedOrder)
        {
            var newOrder = await _dbContext.OrderServices.FirstOrDefaultAsync(p => p.OrderId == id);
            if (newOrder == null)
            {
                return;
            }
            _dbContext.OrderServices.Update(newOrder);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteOrderAsync(string id)
        {
            var newOrder = await _dbContext.OrderServices.FirstOrDefaultAsync(p => p.OrderId == id);
            if (newOrder == null)
            {
                return;
            }
            _dbContext.OrderServices.Remove(newOrder);
            await _dbContext.SaveChangesAsync();
        }
    }
}
