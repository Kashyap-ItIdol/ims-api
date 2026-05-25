using IMS_API.Hubs;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace IMS_API.Services
{
    public class NotificationDispatcher : INotificationDispatcher
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationDispatcher(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task DispatchAsync(int userId, NewNotificationDto data)
        {
            await _hubContext.Clients
                .Group($"User_{userId}")
                .SendAsync("ReceiveNotification", data);
        }

        public Task SendNotificationAsync(int userId, string title, string message)
        {
            var newNotification = new NewNotificationDto
            {
                Title = title,
                Message = message,
                CreatedAt = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            return DispatchAsync(userId, newNotification);
        }
    }
}



