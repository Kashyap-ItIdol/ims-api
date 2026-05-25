using IMS_Application.Common.Constants;
using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Repositories
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsByUserIdAsync(int userId, string? roleName)
        {

            var query = _dbSet.AsQueryable();

            query = roleName switch
            {
                LogicStrings.AdminRole => query
                      .Where(n => !n.IsRead)
                      .OrderByDescending(n => n.CreatedAt),

                _ => _dbSet
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .OrderByDescending(n => n.CreatedAt)
            };

            return await query.ToListAsync();
        }

    }
}

