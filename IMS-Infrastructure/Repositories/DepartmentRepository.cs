using AutoMapper;
using AutoMapper.QueryableExtensions;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Infrastructure.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public DepartmentRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<DepartmentDto>> GetAllDepartmentsAsync()
        {
            return await _context.Department
                .AsNoTracking()
                .ProjectTo<DepartmentDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<DepartmentDto> GetDepartmentByIdAsync(int id)
        {
            return await _context.Department
            .AsNoTracking()
            .Where(r => r.Id == id)
            .ProjectTo<DepartmentDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
        }
    }
}
