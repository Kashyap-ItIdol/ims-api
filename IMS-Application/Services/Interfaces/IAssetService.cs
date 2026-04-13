using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Domain.Entities;

namespace IMS_Application.Services.Interfaces
{
    public interface IAssetService
    {
        Task<Result<string>> AddAssetsAsync(AddAssetDto dto, bool isClient);
        Task<Result<List<AssetResponseDto>>> GetAllAssetsAsync();
        Task<Result<string>> DeleteAssetAsync(int id);

        Task<Result<string>> UpdateAssetAsync(UpdateAssetDto dto);

        //Task<Result<string>> AssignAssetAsync(AssignAssetDto dto);
        //Task<Result<List<User>>> GetSuggestedEmployeesAsync();
        //Task<Result<List<User>>> SearchEmployeesAsync(string query);

    }
}
