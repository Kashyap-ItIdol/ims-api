using IMS_API.Controllers.Base;

using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IMS_API.Controllers
{


    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            // Extract role from JWT claims
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value
                ?? User.FindFirst("role")?.Value;

            currentRole = string.IsNullOrWhiteSpace(currentRole) ? null : currentRole.Trim();

            // Admin can see all unread notifications, regular users see only their own
            var result = await _notificationService.GetUnreadNotificationsAsync(userIdResult.Data, currentRole);
            return FromResult(result);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            // Extract role from JWT claims
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value
                ?? User.FindFirst("role")?.Value;

            currentRole = string.IsNullOrWhiteSpace(currentRole) ? null : currentRole.Trim();

            var result = await _notificationService.MarkAsReadAsync(id, userIdResult.Data, currentRole);
            return FromResult(result);
        }

    }
}
