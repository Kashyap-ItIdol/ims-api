using IMS_Application.DTOs;

namespace IMS_Application.Interfaces
{
    public interface IRoleRepository
    {
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<RoleDto> GetRoleByIdAsync(int id);
    }
}
