using IMS_Application.Common.Models;
using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces
{
    public interface IAssetService
    {
        Task<Result<string>> AddAssetsAsync(AddAssetDto dto, bool isClient);
        Task<Result<List<AssetResponseDto>>> GetAllAssetsAsync();
    }
}
