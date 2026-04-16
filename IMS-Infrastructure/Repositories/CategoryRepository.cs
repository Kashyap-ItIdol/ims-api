using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<List<Category>> GetAllActiveCategoriesAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task<List<string>> GetAllActiveCategoryNamesAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.IsActive)
                .Select(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdWithSubCategoriesAsync(int id)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(c => c.SubCategories.Where(s => s.IsActive))
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}

