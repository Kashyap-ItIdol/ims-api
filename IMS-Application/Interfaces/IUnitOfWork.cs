using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRepository<Department> Departments { get; }
        IRepository<TicketAttachment> TicketAttachments { get; }
        ICategoryRepository Categories { get; }
        ISubCategoryRepository SubCategories { get; }
        ITicketRepository Tickets { get; }
        IAssetRepository Assets { get; }
        INetworkDetailsRepository NetworkDetails { get; }
        IClientAssetRepository ClientAssets { get; }
        IAssetAssignmentRepository AssetAssignments { get; }
        INotificationRepository Notifications { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    }
}
