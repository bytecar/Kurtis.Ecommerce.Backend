using Kurtis.DAL.Interfaces;
using Kurtis.DAL.Repositories;
using System.Threading.Tasks;

namespace Kurtis.DAL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly KurtisDbContext _db;
        private ProductRepository? _products;
        private InventoryRepository? _inventories;

        public UnitOfWork(KurtisDbContext db)
        {
            _db = db;
        }

        public IProductRepository Products => _products ??= new ProductRepository(_db);
        public IInventoryRepository Inventories => _inventories ??= new InventoryRepository(_db);

        public async Task<int> CompleteAsync()
        {
            return await _db.SaveChangesAsync();
        }
    }
}
