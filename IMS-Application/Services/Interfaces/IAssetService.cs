using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces
{
    public interface IAssetService
    {
        Task AddAssetsAsync(AddAssetDto dto, bool isClient);
    }
}
