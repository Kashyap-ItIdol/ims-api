using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface IAssetRepository
    {
        Task AddRangeAsync(List<Asset> assets);
        Task<bool> SerialExistsAsync(string serialNo);
        Task<List<Asset>> GetAllAsync();
        Task<Asset?> GetByIdWithChildrenAsync(int id);
        Task<Asset?> GetByIdAsync(int id);
        Task<Asset?> GetPrimaryAssetByUserIdAsync(int userId);
        Task<bool> SerialExistsAsync(string serialNo, int excludeId);


    }

}

