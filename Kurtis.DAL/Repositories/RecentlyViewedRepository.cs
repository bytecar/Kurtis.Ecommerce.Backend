using Kurtis.Common.Models;
using Kurtis.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kurtis.DAL.Repositories
{
    public class RecentlyViewedRepository : IRecentlyViewedRepository
    {
        private readonly KurtisDbContext _db;

        public RecentlyViewedRepository(KurtisDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<RecentlyViewed>> GetByUserIdAsync(int userId, int count = 10)
        {
            return await _db.RecentlyViewed
                .Where(rv => rv.UserId == userId)
                .OrderByDescending(rv => rv.ViewedAt)
                .Take(count)
                .Include(rv => rv.Product)
                .ThenInclude(p => p.Brand)
                .Include(rv => rv.Product)
                .ThenInclude(p => p.Category)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(RecentlyViewed recentlyViewed)
        {
            // Check if already exists to update timestamp instead of adding duplicate
            var existing = await _db.RecentlyViewed
                .FirstOrDefaultAsync(rv => rv.UserId == recentlyViewed.UserId && rv.ProductId == recentlyViewed.ProductId);

            if (existing != null)
            {
                existing.ViewedAt = DateTime.UtcNow;
                _db.RecentlyViewed.Update(existing);
            }
            else
            {
                await _db.RecentlyViewed.AddAsync(recentlyViewed);
            }
            
            await _db.SaveChangesAsync();
        }

        public async Task DeleteOldRecordsAsync(int userId, int keepCount = 20)
        {
            var records = await _db.RecentlyViewed
                .Where(rv => rv.UserId == userId)
                .OrderByDescending(rv => rv.ViewedAt)
                .Skip(keepCount)
                .ToListAsync();

            if (records.Any())
            {
                _db.RecentlyViewed.RemoveRange(records);
                await _db.SaveChangesAsync();
            }
        }
    }
}
