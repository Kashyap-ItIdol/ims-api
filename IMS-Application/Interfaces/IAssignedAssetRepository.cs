using System.Collections.Generic;
using System.Threading.Tasks;
using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface IAssignedAssetRepository
    {
        Task<IEnumerable<AssignedAsset>> GetAllAsync();
        Task<AssignedAsset?> GetByIdAsync(int id);
        Task AddAsync(AssignedAsset entity);
        Task<bool> UpdateAsync(AssignedAsset entity);
    }
}

