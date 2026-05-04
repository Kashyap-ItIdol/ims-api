using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }

        //IRepository<Department> Departments { get; }

        ICategoryRepository Categories { get; }
        ISubCategoryRepository SubCategories { get; }
        ITicketRepository Tickets { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
