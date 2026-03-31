using IMS_Application.DTOs;
using IMS_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.Services.Interfaces
{
    public interface IRoleService
    {
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<RoleDto> GetRoleByIdAsync(int id);
    }
}
