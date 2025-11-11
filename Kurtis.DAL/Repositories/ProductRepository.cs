
using Kurtis.Common.Models;
using Kurtis.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kurtis.DAL.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly KurtisDbContext _db;
        public ProductRepository(KurtisDbContext db) { _db = db; }

        public async Task AddAsync(Product product)
        {
            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return;
            _db.Products.Remove(p);
            await _db.SaveChangesAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _db.Products.FindAsync(id);            
        }

        public async Task<Product?> GetWithDetailsAsync(int id)
        {
            return await _db.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Inventories)
                .Include(p => p.ProductCollections)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> SearchAsync(string? q, int page, int pageSize)
        {
            var query = _db.Products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => p.Name.Contains(q) || (p.Description != null && p.Description.Contains(q)));
            return await query.OrderByDescending(p => p.CreatedAt)
                              .Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .AsNoTracking().ToListAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
        }
    }
}
