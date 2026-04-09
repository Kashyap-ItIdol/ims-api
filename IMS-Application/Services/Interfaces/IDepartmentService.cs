using IMS_Application.Common.Models;
using IMS_Domain.Entities;

namespace IMS_Application.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<Result<IEnumerable<Department>>> GetAllDepartmentsAsync();
    }
}
