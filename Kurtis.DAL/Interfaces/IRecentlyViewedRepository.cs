using Kurtis.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kurtis.DAL.Interfaces
{
    public interface IRecentlyViewedRepository
    {
        Task<IEnumerable<RecentlyViewed>> GetByUserIdAsync(int userId, int count = 10);
        Task AddAsync(RecentlyViewed recentlyViewed);
        Task DeleteOldRecordsAsync(int userId, int keepCount = 20);
    }
}
