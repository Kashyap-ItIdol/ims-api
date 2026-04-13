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

        public async Task<bool> HasChildrenAsync(int id)
        {
            return await _context.Assets
     .AnyAsync(a => a.ParentAssetId == id && a.IsActive);
        }
        public async Task<Asset?> GetByIdAsync(int id)
        {
            return await _context.Assets
    .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        }

        public void SoftDelete(Asset asset)
        {
            asset.IsActive = false;
        }

        //public async Task<List<User>> SearchAsync(string query)
        //{
        //    return await _context.Users
        //        .Where(u => u.FullName.Contains(query) && !u.IsDeleted)
        //        .ToListAsync();
        //}

        //public async Task<bool> TableAlreadyAssignedAsync(string tableNo)
        //{
        //    return await _context.Users
        //        .AnyAsync(u => u.TableNo == tableNo && !u.IsDeleted);
        //}
    }
}