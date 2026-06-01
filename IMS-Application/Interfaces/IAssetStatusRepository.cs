using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface IAssetStatusRepository
    {
        Task<AssetStatus?> GetByStatusNameAsync(string statusName);
        Task<List<AssetStatus>> GetAllAsync();
    }
}

