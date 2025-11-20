using Kurtis.Common.Models;
using Kurtis.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Kurtis.DAL.Repositories
{
    public class UserPreferencesRepository : IUserPreferencesRepository
    {
        private readonly KurtisDbContext _db;

        public UserPreferencesRepository(KurtisDbContext db)
        {
            _db = db;
        }

        public async Task<UserPreferences?> GetByUserIdAsync(int userId)
        {
            return await _db.UserPreferences
                .FirstOrDefaultAsync(up => up.UserId == userId);
        }

        public async Task AddAsync(UserPreferences preferences)
        {
            await _db.UserPreferences.AddAsync(preferences);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserPreferences preferences)
        {
            _db.UserPreferences.Update(preferences);
            await _db.SaveChangesAsync();
        }
    }
}
