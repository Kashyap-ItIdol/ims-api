using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IDepartmentRepository Departments { get; }

        ICategoryRepository Categories { get; }
        ISubCategoryRepository SubCategories { get; }
        ITicketRepository Tickets { get; }
        IAssetRepository Assets { get; }
        INetworkDetailsRepository NetworkDetails { get; }
        IClientAssetRepository ClientAssets { get; }
        IAssetAssignmentRepository AssetAssignments { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
