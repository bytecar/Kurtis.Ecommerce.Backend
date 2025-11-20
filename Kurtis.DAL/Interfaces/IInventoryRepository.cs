using Kurtis.Common.Models;

namespace Kurtis.DAL.Interfaces
{
    public interface IInventoryRepository
    {
        Task AddAsync(Inventory inv);
        Task<Inventory?> GetByIdAsync(int id);
        Task<IEnumerable<Inventory>> GetByProductIdAsync(int productId);
        Task<bool> DecrementStockAsync(int productId, string size, int qty);
        Task UpdateAsync(Inventory inv);
        Task DeleteAsync(int id);

    }
}