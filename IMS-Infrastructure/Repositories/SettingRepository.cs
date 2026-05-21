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

        public Task<List<RecentActivity>> GetRecentActivitiesAsync(int pageNumber, int pageSize, string? search)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim();
                query = query.Where(x =>
                    (x.ItemName != null && x.ItemName.Contains(q)) ||
                    (x.Action != null && x.Action.Contains(q)) ||
                    (x.Details != null && x.Details.Contains(q)) ||
                    (x.User != null && x.User.FullName != null && x.User.FullName.Contains(q)));
            }

            return query
                .OrderByDescending(x => x.DateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public Task<int> GetRecentActivitiesTotalCountAsync(string? search)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim();
                query = query.Where(x =>
                    (x.ItemName != null && x.ItemName.Contains(q)) ||
                    (x.Action != null && x.Action.Contains(q)) ||
                    (x.Details != null && x.Details.Contains(q)));
            }

            return query.CountAsync();
        }

        public Task<List<RecentActivity>> GetDeletedRecentActivitiesAsync(int pageNumber, int pageSize, string? search)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim();
                query = query.Where(x =>
                    (x.ItemName != null && x.ItemName.Contains(q)) ||
                    (x.Action != null && x.Action.Contains(q)) ||
                    (x.Details != null && x.Details.Contains(q)) ||
                    (x.User != null && x.User.FullName != null && x.User.FullName.Contains(q)));
            }

            return query
                .OrderByDescending(x => x.DateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public Task<int> GetDeletedRecentActivitiesTotalCountAsync(string? search)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(x => x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim();
                query = query.Where(x =>
                    (x.ItemName != null && x.ItemName.Contains(q)) ||
                    (x.Action != null && x.Action.Contains(q)) ||
                    (x.Details != null && x.Details.Contains(q)));
            }

            return query.CountAsync();
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
