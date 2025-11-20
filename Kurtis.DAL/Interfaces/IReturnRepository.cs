using Kurtis.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kurtis.DAL.Interfaces
{
    public interface IReturnRepository
    {
        Task<Return?> GetByIdAsync(int id);
        Task<IEnumerable<Return>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Return>> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<Return>> GetAllAsync();
        Task AddAsync(Return returnRequest);
        Task UpdateAsync(Return returnRequest);
    }
}
