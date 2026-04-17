using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace IMS_Infrastructure.Repositories
{
    public class AssetRepository : IAssetRepository
    {
        private readonly AppDbContext _context;

        public AssetRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(List<Asset> assets)
        {
            await _context.Set<Asset>().AddRangeAsync(assets);
        }

        public async Task<bool> SerialExistsAsync(string serialNo)
        {
            return await _context.Set<Asset>()
                .AnyAsync(x => x.SerialNo == serialNo);
        }

        public async Task<List<Asset>> GetAllAsync()
        {

            return await _context.Set<Asset>()
                 .Where(a => a.IsActive) 
                .Include(a => a.AssignedUser)
                .Include(a => a.ChildAssets.Where(c => c.IsActive)) 
                 .ToListAsync();
        }

        public async Task<Asset?> GetByIdAsync(int id)
        {
            return await _context.Set<Asset>()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        }

        public async Task<Asset?> GetByIdWithChildrenAsync(int id)
        {
            return await _context.Set<Asset>()
                .Include(x => x.AssignedUser)
                .Include(x => x.ChildAssets)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        }

        public async Task<Asset?> GetPrimaryAssetByUserIdAsync(int userId)
        {
            return await _context.Set<Asset>()
                .FirstOrDefaultAsync(x => x.AssignedTo == userId && x.ParentAssetId == null);
        }

        public async Task<bool> SerialExistsAsync(string serialNo, int excludeId)
        {
            return await _context.Set<Asset>()
                .AnyAsync(x => x.SerialNo == serialNo && x.Id != excludeId);
        }

        public async Task AddAsync(AssetHistory history)
        {
            await _context.AssetHistories.AddAsync(history);
        }

        public async Task<List<AssetHistory>> GetByAssetIdAsync(int assetId)
        {
            return await _context.AssetHistories
                .Where(x => x.AssetId == assetId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }



    }
}