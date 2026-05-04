using IMS_Application.Common.Models;
using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces;

public interface IAssetAssignmentService
{
    Task<Result<AssetAssignmentResponseDto>> AssignAssetAsync(AssetAssignmentDto dto, int createdBy);
    Task<Result<List<AssetAssignmentResponseDto>>> GetAllAsync();
    Task<Result<AssetAssignmentResponseDto>> GetByIdAsync(int id);
    Task<Result<AssetAssignmentResponseDto>> UpdateAssetAsync(int id, AssetAssignmentDto dto, int updatedBy);
    Task<Result<AssetAssignmentResponseDto>> ReturnAssetAsync(int id, DateTime returnDate, int updatedBy);
    Task<Result<AssetAssignmentResponseDto>> DeleteAssetAsync(int id, int updatedBy);
    Task<Result<AssetAssignmentResponseDto>> CreateAndAssignAssetAsync(CreateAndAssignAssetDto dto, int createdBy);
}
