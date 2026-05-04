using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface IAssetRepository
    {
        Task Add(Asset asset);
        Task<IEnumerable<Asset>> GetAll();
        Task<Asset?> GetById(int id);
        Task Update(Asset asset);
        Task Delete(Asset asset);
        Task Save();
    }
}
