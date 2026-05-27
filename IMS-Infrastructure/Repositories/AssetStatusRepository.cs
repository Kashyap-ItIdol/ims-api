using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Repositories
{
    public class AssetStatusRepository : IAssetStatusRepository
    {
        private readonly AppDbContext _context;

        public AssetStatusRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AssetStatus?> GetByStatusNameAsync(string statusName)
        {
            var normalized = statusName.Trim().ToLower();

            return await _context.AssetStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.Status != null &&
                    s.Status.Trim().ToLower() == normalized);
        }

        public async Task<List<AssetStatus>> GetAllAsync()
        {
            return await _context.AssetStatuses
                .AsNoTracking()
                .ToListAsync();
        }
    }
}

