using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface IDepartmentRepository : IRepository<Department>
    {
        new Task<Department?> GetByIdAsync(int id);
    }
}
