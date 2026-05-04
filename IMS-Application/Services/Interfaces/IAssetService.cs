using IMS_Application.Common.Models;
using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces
{
    public interface IAssetService
    {
        Task<Result<AssetResponseDto>> Create(CreateAssetDto dto, int createdBy);
        Task<Result<List<AssetResponseDto>>> GetAll();
        Task<Result<AssetResponseDto>> GetById(int id);
        Task<Result<AssetResponseDto>> Update(int id, CreateAssetDto dto, int updatedBy);
        Task<Result<AssetResponseDto>> Delete(int id, int deletedBy);
    }
}
