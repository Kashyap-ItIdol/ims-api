using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces
{
    public interface IRoleService
    {
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<RoleDto> GetRoleByIdAsync(int id);
    }
}
