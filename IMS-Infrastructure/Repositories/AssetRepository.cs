using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Repositories
{
    public class AssetRepository : Repository<Asset>, IAssetRepository
    {
        public AssetRepository(AppDbContext context) : base(context)
        {
        }

        public async Task Add(Asset asset)
        {
            await AddAsync(asset);
        }

        public async Task<IEnumerable<Asset>> GetAll()
        {
            return await _context.Assets
                .Include(a => a.Category)
                .Include(a => a.SubCategory)
                .Where(a => !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<Asset?> GetById(int id)
        {
            if (id <= 0)
                return null;

            return await _context.Assets
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public new async Task Update(Asset asset)
        {
            base.Update(asset);
            await Task.CompletedTask;
        }

        public async Task Delete(Asset asset)
        {
            Remove(asset);
            await Task.CompletedTask;
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}
