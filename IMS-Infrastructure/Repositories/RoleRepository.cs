using AutoMapper;
using AutoMapper.QueryableExtensions;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public RoleRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            return await _context.Roles
                .AsNoTracking()
                .ProjectTo<RoleDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<RoleDto> GetRoleByIdAsync(int id)
        {
            var result = await _context.Roles
            .AsNoTracking()
            .Where(r => r.Id == id)
            .ProjectTo<RoleDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

            return result ?? throw new Exception("Role not found");
        }
    }
}
