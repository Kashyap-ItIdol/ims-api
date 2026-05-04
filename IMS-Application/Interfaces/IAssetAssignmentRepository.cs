using IMS_Domain.Entities;

namespace IMS_Application.Interfaces;

public interface IAssetAssignmentRepository
{
    Task<AssetAssignment> AddAsync(AssetAssignment entity);
    Task<AssetAssignment?> GetByIdAsync(int id);
    Task<IEnumerable<AssetAssignment>> GetAllAsync();
    Task UpdateAsync(AssetAssignment entity);
    Task DeleteAsync(int id);
}