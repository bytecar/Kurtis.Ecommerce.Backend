
using Kurtis.Common.Models;
using Kurtis.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kurtis.DAL.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly KurtisDbContext _db;
        public InventoryRepository(KurtisDbContext db) { _db = db; }

        public async Task AddAsync(Inventory inv)
        {
            await _db.Inventories.AddAsync(inv);
            await _db.SaveChangesAsync();
        }

        public async Task<Inventory?> GetByIdAsync(int id)
        {
            return await _db.Inventories.FindAsync(id);
        }

        public async Task<IEnumerable<Inventory>> GetByProductIdAsync(int productId)
        {
            return await _db.Inventories.Where(i => i.ProductId == productId).AsNoTracking().ToListAsync();
        }

        public async Task<bool> DecrementStockAsync(int productId, string size, int qty)
        {
            var item = await _db.Inventories.FromSqlInterpolated($@"
                SELECT * FROM dbo.Inventories WITH (UPDLOCK, ROWLOCK) WHERE ProductId = {productId} AND Size = {size}
            ").FirstOrDefaultAsync();
            if (item == null) return false;
            if (item.Quantity < qty) return false;
            item.Quantity -= qty;
            item.UpdatedAt = System.DateTime.UtcNow;
            _db.Inventories.Update(item);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task UpdateAsync(Inventory inv)
        {
            _db.Inventories.Update(inv);
            await _db.SaveChangesAsync();
        }
    }


}
