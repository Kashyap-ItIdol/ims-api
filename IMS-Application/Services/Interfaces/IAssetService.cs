using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Domain.Entities;

namespace IMS_Application.Services.Interfaces
{
    public interface IAssetService
    {
        Task<Result<string>> AddAssetsAsync(AddAssetDto dto, bool isClient);
        Task<Result<List<AssetResponseDto>>> GetAllAssetsAsync();

        Task<Result<string>> UpdateAssetAsync(UpdateAssetDto dto);

        Task<Result<string>> DeleteAssetAsync(int id);

        Task<Result<List<UserDto>>> GetSuggestedUsersAsync();
        Task<Result<List<UserDto>>> SearchUsersAsync(string query);

        Task<Result<string>> AssignAssetAsync(AssignAssetDto dto);
        Task<Result<GetAssetByIdResponseDto>> GetAssetByIdAsync(int id);

        Task<Result<string>> AttachChildAsync(AttachChildDto dto);

        Task<Result<string>> CreateAndAttachChildAsync(CreateChildAssetDto dto);

        Task<Result<string>> DetachChildAsync(DetachChildDto dto);
        Task<Result<List<AssetListDto>>> FilterAssetsAsync(AssetFilterDto dto);
    }  
}
