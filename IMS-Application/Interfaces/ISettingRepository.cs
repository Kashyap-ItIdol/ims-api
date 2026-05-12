using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface ISettingRepository : IRepository<RecentActivity>
    {
        Task AddRecentActivityAsync(RecentActivity activity);

        Task<List<RecentActivity>> GetRecentActivitiesAsync(int pageNumber, int pageSize);

        Task<int> GetRecentActivitiesTotalCountAsync();

        Task<List<RecentActivity>> GetDeletedRecentActivitiesAsync(int pageNumber, int pageSize);

        Task<int> GetDeletedRecentActivitiesTotalCountAsync();
    }
}