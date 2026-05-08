using IMS_Application.DTOs;
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
        Task<List<Asset>> FilterAsync(AssetFilterDto dto);
        Task AddHistoryAsync(AssetHistory history);
        Task<List<AssetHistory>> GetHistoryByAssetIdAsync(int assetId);
        Task<List<AssetHistory>> GetHistoryByAssetIdsAsync(List<int> assetIds);
        Task Add(Asset asset);
        Task<IEnumerable<Asset>> GetAll();
        Task<Asset?> GetById(int id);
        Task Update(Asset asset);
        Task Delete(Asset asset);
        Task Save();
    }
}
