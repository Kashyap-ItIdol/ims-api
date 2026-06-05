using AutoMapper;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace IMS_Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<NotificationDto>>> GetUnreadNotificationsAsync(int userId, string? roleName = null)
        {
            try
            {
                // Role-aware filtering is handled in the repository.
                var notifications = await _unitOfWork.Notifications
                    .GetUnreadNotificationsByUserIdAsync(userId, roleName);

                var notificationDtos = _mapper.Map<List<NotificationDto>>(notifications);

                _logger.LogInformation(
                    "Retrieved {Count} unread notifications for user {UserId} (role: {Role})",
                    notificationDtos.Count,
                    userId,
                    roleName ?? "None");

                return Result<List<NotificationDto>>.Success(
                    notificationDtos,
                    SuccessMessages.RetrievedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications for user {UserId}", userId);
                return Result<List<NotificationDto>>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<NotificationDto>> MarkAsReadAsync(int notificationId, int userId, string? roleName = null)
        {
            try
            {
                var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);

                if (notification == null)
                {
                    _logger.LogWarning("Notification {NotificationId} not found", notificationId);
                    return Result<NotificationDto>.Failure(ErrorMessages.ServerError, 404);
                }

                // Admin can mark any notification as read, regular users can only mark their own
                if (notification.UserId != userId && roleName != LogicStrings.AdminRole)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to mark notification {NotificationId} owned by another user",
                        userId,
                        notificationId);
                    return Result<NotificationDto>.Failure(ErrorMessages.Unauthorized, 403);
                }

                notification.IsRead = true;
                _unitOfWork.Notifications.Update(notification);
                await _unitOfWork.SaveChangesAsync();

                var notificationDto = _mapper.Map<NotificationDto>(notification);

                _logger.LogInformation(
                    "Notification {NotificationId} marked as read by user {UserId}",
                    notificationId,
                    userId);

                return Result<NotificationDto>.Success(notificationDto, SuccessMessages.RetrievedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error marking notification {NotificationId} as read by user {UserId}",
                    notificationId,
                    userId);
                return Result<NotificationDto>.Failure(ErrorMessages.ServerError, 500);
            }
        }
    }
}

