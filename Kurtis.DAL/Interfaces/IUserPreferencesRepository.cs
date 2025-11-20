using Kurtis.Common.Models;
using System.Threading.Tasks;

namespace Kurtis.DAL.Interfaces
{
    public interface IUserPreferencesRepository
    {
        Task<UserPreferences?> GetByUserIdAsync(int userId);
        Task AddAsync(UserPreferences preferences);
        Task UpdateAsync(UserPreferences preferences);
    }
}
