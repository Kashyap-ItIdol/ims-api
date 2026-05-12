using IMS_Application.Common.Models;
using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<Result<IEnumerable<DepartmentDto>>> GetAllDepartmentsAsync();
    }
}
