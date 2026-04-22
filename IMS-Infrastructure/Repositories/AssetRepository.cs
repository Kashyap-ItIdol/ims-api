using IMS_Application.DTOs;
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
            return await _context.Assets
       .Include(a => a.AssetStatus)
       .Include(a => a.Category)
       .Include(a => a.SubCategory)
       .Include(a => a.AssetCondition)
       .Include(a => a.AssignedUser)
           .ThenInclude(u => u.Department)
       .Include(a => a.ChildAssets)
       .FirstOrDefaultAsync(a => a.Id == id);
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

        public async Task<List<Asset>> FilterAsync(AssetFilterDto dto)
        {
            var query = _context.Assets
                .Include(a => a.Category)
                .Include(a => a.SubCategory)
                .Include(a => a.AssetStatus)
                .Include(a => a.AssignedUser)
                .Where(a => a.IsActive);

            // Normal filters
            if (dto.CategoryIds != null && dto.CategoryIds.Any())
                query = query.Where(a => dto.CategoryIds.Contains(a.CategoryId));

            if (dto.SubCategoryIds != null && dto.SubCategoryIds.Any())
                query = query.Where(a => dto.SubCategoryIds.Contains(a.SubCategoryId));

            if (dto.StatusIds != null && dto.StatusIds.Any())
                query = query.Where(a => dto.StatusIds.Contains(a.StatusId));

            // Context-based search
            if (!string.IsNullOrWhiteSpace(dto.Search) && !string.IsNullOrWhiteSpace(dto.SearchType))
            {
                var search = dto.Search.ToLower();

                switch (dto.SearchType.ToLower())
                {
                    case "category":
                        query = query.Where(a => a.Category.Name.ToLower().Contains(search));
                        break;

                    case "subcategory":
                        query = query.Where(a => a.SubCategory.Name.ToLower().Contains(search));
                        break;

                    case "status":
                        query = query.Where(a => a.AssetStatus.Status.ToLower().Contains(search));
                        break;
                }
            }

            return await query.ToListAsync();
        }
    }
}