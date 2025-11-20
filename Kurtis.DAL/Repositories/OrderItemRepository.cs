using Kurtis.Common.Models;
using Kurtis.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kurtis.DAL.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly KurtisDbContext _db;

        public OrderItemRepository(KurtisDbContext db)
        {
            _db = db;
        }

        public async Task<OrderItem?> GetByIdAsync(int id)
        {
            return await _db.OrderItems
                .Include(oi => oi.Product)
                .FirstOrDefaultAsync(oi => oi.Id == id);
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId)
        {
            return await _db.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Include(oi => oi.Product)
                .ToListAsync();
        }

        public async Task AddAsync(OrderItem orderItem)
        {
            await _db.OrderItems.AddAsync(orderItem);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(OrderItem orderItem)
        {
            _db.OrderItems.Update(orderItem);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _db.OrderItems.FindAsync(id);
            if (item != null)
            {
                _db.OrderItems.Remove(item);
                await _db.SaveChangesAsync();
            }
        }
    }
}
