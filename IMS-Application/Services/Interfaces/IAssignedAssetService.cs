using System.Collections.Generic;
using System.Threading.Tasks;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces
{
    public interface IAssignedAssetService
    {
        Task<Result<ClientAssignedAssetDto>> CreateAsync(CreateClientAssignedAssetDto dto, int userId);
        Task<Result<IEnumerable<ClientAssignedAssetDto>>> GetAllAsync();
        Task<Result<ClientAssignedAssetDto>> GetByIdAsync(int id);
        Task<Result<bool>> UpdateAsync(int id, UpdateClientAssignedAssetDto dto, int userId);
        Task<Result<bool>> DeleteAsync(int id, int deletedBy);

    }
}


