using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Domain.Entities;

namespace IMS_Application.Services.Interfaces
{
    public interface IAssetService
    {
        Task<Result<string>> AddAssetsAsync(AddAssetDto dto, int createdBy);
        Task<Result<List<AssetResponseDto>>> GetAllAssetsAsync();

        Task<Result<string>> UpdateAssetAsync(UpdateAssetDto dto, int updatedBy);

        Task<Result<string>> DeleteAssetAsync(int id, int deletedBy);

        Task<Result<List<UserDto>>> GetSuggestedUsersAsync();
        Task<Result<List<UserDto>>> SearchUsersAsync(string query);

        Task<Result<string>> AssignAssetAsync(AssignAssetDto dto);
        Task<Result<GetAssetByIdResponseDto>> GetAssetByIdAsync(int id);

        Task<Result<string>> AttachChildAsync(AttachChildDto dto);

        Task<Result<string>> CreateAndAttachChildAsync(CreateChildAssetDto dto);

        Task<Result<string>> DetachChildAsync(DetachChildDto dto);
        Task<Result<List<AssetListDto>>> FilterAssetsAsync(AssetFilterDto dto);

        Task<Result<string>> AddOrUpdateNetworkAsync(int assetId, NetworkDetailsDto dto, int userId);
    }  
}
