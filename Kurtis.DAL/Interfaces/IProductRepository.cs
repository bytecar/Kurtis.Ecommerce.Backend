using Kurtis.Common.Models;

namespace Kurtis.DAL.Interfaces
{
    public interface IProductRepository
    {
        Task AddAsync(Product product);
        Task DeleteAsync(int id);
        Task<Product?> GetWithDetailsAsync(int id);
        Task<IEnumerable<Product>> SearchAsync(string? q, int page, int pageSize);
        Task UpdateAsync(Product product);
        Task<Product?> GetByIdAsync(int id);

    }
}