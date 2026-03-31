using IMS_Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.Interfaces
{
    public interface IRoleRepository
    {
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<RoleDto> GetRoleByIdAsync(int id);
    }
}
