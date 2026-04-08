using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }

        IRepository<Department> Departments { get; }

        ICategoryRepository Categories { get; }
        // As you add more modules, you add them here (e.g., IProductRepository Products { get; })

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
