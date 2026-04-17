using IMS_Domain.Entities;


namespace IMS_Application.Interfaces
{
    public interface IAssetHistoryRepository
    {
        Task AddAsync(AssetHistory history);
        Task<List<AssetHistory>> GetByAssetIdAsync(int assetId);
    }
}
