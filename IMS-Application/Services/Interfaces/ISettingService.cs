using IMS_Application.Common.Models;
using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces
{
    public interface ISettingService
    {
        Task<Result<PagedResult<RecentActivityItemDto>>> GetRecentActivitiesAsync(int pageNumber, int pageSize);

        Task<Result<PagedResult<RecentActivityItemDto>>> GetRecentDeletedActivitiesAsync(int pageNumber, int pageSize);
    }
}

