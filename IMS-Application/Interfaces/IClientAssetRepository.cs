using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface IClientAssetRepository : IRepository<ClientAsset>
    {
        new Task<ClientAsset?> GetByIdAsync(int id);
        Task<ClientAsset?> GetWithAttachmentsAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> UpdateAsync(ClientAsset asset);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ClientAsset>> FilterAsync(ClientAssetFilterDto filter);
        
        Task AddAttachmentAsync(ClientAssetAttachment attachment);
        Task<ClientAssetAttachment?> GetAttachmentByIdAsync(int id);
        Task<IEnumerable<ClientAssetAttachment>> GetAttachmentsByAssetIdAsync(int assetId);
        Task DeleteAttachmentAsync(ClientAssetAttachment attachment);
        Task SaveChangesAsync();
    }
}