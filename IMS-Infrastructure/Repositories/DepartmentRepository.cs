using IMS_Application.Common.Models;
using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Repositories
{
    public class DepartmentRepository : Repository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(AppDbContext context) : base(context)
        {
        }

        public new async Task<Department?> GetByIdAsync(int id)
            => await _dbSet.Include(d => d.Users).FirstOrDefaultAsync(d => d.Id == id);

        public new async Task<IEnumerable<Department>> GetAllAsync()
            => await _dbSet.Include(d => d.Users).AsNoTracking().ToListAsync();

        public new async Task<PagedResult<Department>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var totalCount = await _dbSet.CountAsync();
            var items = await _dbSet
                .Include(d => d.Users)
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Department>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
