using IMS_Application.Common.Models;
using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces
{
    public interface INotificationService
    {
        Task<Result<List<NotificationDto>>> GetUnreadNotificationsAsync(int userId, string? roleName = null);
        Task<Result<NotificationDto>> MarkAsReadAsync(int notificationId, int userId, string? roleName = null);
    }
}
