using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace IMS_API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly INotificationService _notificationService;

        public NotificationHub(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?
                .FindFirst(ClaimTypes.NameIdentifier)
                ?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                var groupName = $"User_{userId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                // Push missed/unread notifications on connect
                var result = await _notificationService.GetUnreadNotificationsAsync(int.Parse(userId));

                if (result.IsSuccess && result.Data != null)
                {
                    foreach (NotificationDto notification in result.Data)
                    {
                        await Clients.Caller.SendAsync(
                            "ReceiveNotification",
                            new NewNotificationDto
                            {
                                Title = notification.Title,
                                Message = notification.Message,
                                CreatedAt = notification.CreatedAt

                            }

                        );

                    }
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?
                .FindFirst(ClaimTypes.NameIdentifier)
                ?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}



