using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Repositories
{
    public class AssetHistoryRepository : IAssetHistoryRepository
    {
        private readonly AppDbContext _context;

        public AssetHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AssetHistory history)
        {
            await _context.Set<AssetHistory>().AddAsync(history);
        }

        public async Task<List<AssetHistory>> GetByAssetIdAsync(int assetId)
        {
            return await _context.Set<AssetHistory>()
                .Where(x => x.AssetId == assetId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}
