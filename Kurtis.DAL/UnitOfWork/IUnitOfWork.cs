using Kurtis.DAL.Interfaces;

namespace Kurtis.DAL.UnitOfWork
{
    public interface IUnitOfWork
    {
        IProductRepository Products { get; }
        IInventoryRepository Inventories { get; }
        Task<int> CompleteAsync();
    }
}
