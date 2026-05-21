using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Repositories
{
    public class SettingRepository : Repository<RecentActivity>, ISettingRepository
    {
        public SettingRepository(AppDbContext context) : base(context)
        {
        }

        public async Task AddRecentActivityAsync(RecentActivity activity)
        {
            await _dbSet.AddAsync(activity);
        }

        public Task<List<RecentActivity>> GetRecentActivitiesAsync(int pageNumber, int pageSize)
        {
            return _dbSet
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.DateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public Task<int> GetRecentActivitiesTotalCountAsync()
        {
            return _dbSet
                .AsNoTracking()
                .CountAsync(x => !x.IsDeleted);
        }

        public Task<List<RecentActivity>> GetDeletedRecentActivitiesAsync(int pageNumber, int pageSize)
        {
            return _dbSet
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => x.IsDeleted)
                .OrderByDescending(x => x.DateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public Task<int> GetDeletedRecentActivitiesTotalCountAsync()
        {
            return _dbSet
                .AsNoTracking()
                .CountAsync(x => x.IsDeleted);
        }
        public Task<List<RecentActivity>> GetUserActivitiesAsync(int userId, DateTime? startDate, DateTime? endDate)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => !x.IsDeleted && x.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(x => x.DateTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.DateTime <= endDate.Value);

            return query
                .OrderByDescending(x => x.DateTime)
                .ToListAsync();
        }
    }
}
