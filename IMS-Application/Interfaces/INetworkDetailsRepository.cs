using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface INetworkDetailsRepository
    {
        Task<NetworkDetail?> GetByAssetIdAsync(int assetId);
    }
}
