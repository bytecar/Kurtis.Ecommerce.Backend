
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kurtis.DAL.Dapper.Interfaces
{
    public interface ICatalogDapperRepository
    {
        Task<IEnumerable<dynamic>> SearchProductsAsync(string? q, int page = 1, int pageSize = 20);
        Task<dynamic?> GetProductDetailsAsync(int productId);
    }
}
