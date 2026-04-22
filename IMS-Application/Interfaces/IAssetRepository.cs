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

        Task AddAsync(AssetHistory history);
        Task<List<AssetHistory>> GetByAssetIdAsync(int assetId);
        Task<List<Asset>> FilterAsync(AssetFilterDto dto);


    }

}

