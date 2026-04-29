using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Repositories
{
    public class SubCategoryRepository : Repository<SubCategory>, ISubCategoryRepository
    {
        public SubCategoryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<SubCategory?> GetByNameAsync(string name)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(sc => sc.Name.ToLower() == name.ToLower());
        }

        public async Task<SubCategory?> GetByCategoryIdAndNameAsync(int categoryId, string name)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(sc => sc.CategoryId == categoryId && sc.Name.ToLower() == name.ToLower());
        }
    }
}
