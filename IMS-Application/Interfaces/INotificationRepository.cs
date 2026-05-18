using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetUnreadNotificationsByUserIdAsync(int userId, string? roleName);
    }
}

