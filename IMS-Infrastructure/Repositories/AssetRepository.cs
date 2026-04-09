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

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}