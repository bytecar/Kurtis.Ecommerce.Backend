using Kurtis.Common.Models;
using Kurtis.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kurtis.DAL.Repositories
{
    public class ReturnRepository : IReturnRepository
    {
        private readonly KurtisDbContext _db;

        public ReturnRepository(KurtisDbContext db)
        {
            _db = db;
        }

        public async Task<Return?> GetByIdAsync(int id)
        {
            return await _db.Returns
                .Include(r => r.Order)
                .Include(r => r.OrderItem)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Return>> GetByUserIdAsync(int userId)
        {
            return await _db.Returns
                .Include(r => r.Order)
                .Where(r => r.Order.UserId == userId)
                .Include(r => r.OrderItem)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Return>> GetByOrderIdAsync(int orderId)
        {
            return await _db.Returns
                .Where(r => r.OrderId == orderId)
                .Include(r => r.OrderItem)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<Return>> GetAllAsync()
        {
            return await _db.Returns
                .Include(r => r.Order)
                .Include(r => r.OrderItem)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Return returnRequest)
        {
            await _db.Returns.AddAsync(returnRequest);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Return returnRequest)
        {
            _db.Returns.Update(returnRequest);
            await _db.SaveChangesAsync();
        }
    }
}
