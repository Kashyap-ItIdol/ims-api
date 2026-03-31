using AutoMapper;
using AutoMapper.QueryableExtensions;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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
            return await _context.Roles
            .AsNoTracking()
            .Where(r => r.Id == id)
            .ProjectTo<RoleDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
        }
    }
}
