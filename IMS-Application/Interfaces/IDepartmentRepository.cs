using IMS_Application.DTOs;

namespace IMS_Application.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<List<DepartmentDto>> GetAllDepartmentsAsync();
        Task<DepartmentDto> GetDepartmentByIdAsync(int id);
    }
}
