using IMS_Application.Common.Models;
using IMS_Application.Interfaces;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
                    => await _dbSet.FindAsync(id);
        public async Task<IEnumerable<T>> GetAllAsync()
                    => await _dbSet.AsNoTracking().ToListAsync();

        public async Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var totalCount = await _dbSet.CountAsync();
            var items = await _dbSet
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Remove(T entity) => _dbSet.Remove(entity);

       
    }
}
