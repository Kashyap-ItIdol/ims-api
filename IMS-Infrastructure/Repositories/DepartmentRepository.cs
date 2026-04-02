using AutoMapper;
using AutoMapper.QueryableExtensions;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
            return await _context.Departments
                .AsNoTracking()
                .ProjectTo<DepartmentDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<DepartmentDto> GetDepartmentByIdAsync(int id)
        {
            var result =  await _context.Departments
            .AsNoTracking()
            .Where(r => r.Id == id)
            .ProjectTo<DepartmentDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

            return result ?? throw new Exception("Department not found");
        }
    }
}
