using IMS_Application.DTOs;
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

        public async Task AddRangeAsync(List<Asset> assets)
        {
            await _dbSet.AddRangeAsync(assets);
        }

        public async Task<bool> SerialExistsAsync(string serialNo)
        {
            return await _dbSet
                .AsNoTracking()
                .AnyAsync(x => x.SerialNo == serialNo);
        }

        public async Task<List<Asset>> GetAllAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(a => a.IsActive)
                .Include(a => a.AssignedUser)
                .Include(a => a.ChildAssets.Where(c => c.IsActive))
                .ToListAsync();
        }

        public async Task<Asset?> GetByIdAsync(int id)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        }

        public async Task<Asset?> GetByIdWithChildrenAsync(int id)
        {
            return await _dbSet
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
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.AssignedTo == userId && x.ParentAssetId == null);
        }

        public async Task<bool> SerialExistsAsync(string serialNo, int excludeId)
        {
            return await _dbSet
                .AsNoTracking()
                .AnyAsync(x => x.SerialNo == serialNo && x.Id != excludeId);
        }


        public async Task AddHistoryAsync(AssetHistory history)
        {
            await _context.Set<AssetHistory>().AddAsync(history);
        }

        public async Task<List<AssetHistory>> GetHistoryByAssetIdAsync(int assetId)
        {
            return await _context.Set<AssetHistory>()
                .AsNoTracking()
                .Where(x => x.AssetId == assetId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Asset>> FilterAsync(AssetFilterDto dto)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(a => a.Category)
                .Include(a => a.SubCategory)
                .Include(a => a.AssetStatus)
                .Include(a => a.AssignedUser)
                .Where(a => a.IsActive);

            if (dto.CategoryIds?.Any() == true)
                query = query.Where(a => dto.CategoryIds.Contains(a.CategoryId));

            if (dto.SubCategoryIds?.Any() == true)
                query = query.Where(a => dto.SubCategoryIds.Contains(a.SubCategoryId));

            if (dto.StatusIds?.Any() == true)
                query = query.Where(a => dto.StatusIds.Contains(a.StatusId));

           
            if (!string.IsNullOrWhiteSpace(dto.Search) && !string.IsNullOrWhiteSpace(dto.SearchType))
            {
                var search = dto.Search;

                switch (dto.SearchType.ToLower())
                {
                    case "category":
                        query = query.Where(a => EF.Functions.Like(a.Category.Name, $"%{search}%"));
                        break;

                    case "subcategory":
                        query = query.Where(a => EF.Functions.Like(a.SubCategory.Name, $"%{search}%"));
                        break;

                    case "status":
                        query = query.Where(a => EF.Functions.Like(a.AssetStatus.Status, $"%{search}%"));
                        break;
                }
            }

            return await query.ToListAsync();
        }
    }
}