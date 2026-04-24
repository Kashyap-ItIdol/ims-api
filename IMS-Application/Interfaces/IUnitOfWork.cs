using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRepository<Department> Departments { get; }
        IAssetRepository Assets { get; }
        INetworkDetailsRepository NetworkDetails { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
