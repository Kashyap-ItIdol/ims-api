using IMS_Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<DepartmentDto>> GetAllDepartmentsAsync();
        Task<DepartmentDto> GetDepartmentByIdAsync(int id);
    }
}
