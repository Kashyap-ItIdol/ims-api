using IMS_Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<List<DepartmentDto>> GetAllDepartmentsAsync();
        Task<DepartmentDto> GetDepartmentByIdAsync(int id);
    }
}
