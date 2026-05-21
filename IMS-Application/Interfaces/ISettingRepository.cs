using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface ISettingRepository : IRepository<RecentActivity>
    {
        Task AddRecentActivityAsync(RecentActivity activity);
        Task<List<RecentActivity>> GetRecentActivitiesAsync(int pageNumber, int pageSize, string? search);
        Task<int> GetRecentActivitiesTotalCountAsync(string? search);
        Task<List<RecentActivity>> GetDeletedRecentActivitiesAsync(int pageNumber, int pageSize, string? search);
        Task<int> GetDeletedRecentActivitiesTotalCountAsync(string? search);
        Task<List<RecentActivity>> GetUserActivitiesAsync(int userId, DateTime? startDate, DateTime? endDate);
    }
}