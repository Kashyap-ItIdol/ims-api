using System.Collections.Generic;
using System.Threading.Tasks;
using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Repositories
{
    public class AssignedAssetRepository : IAssignedAssetRepository
    {
        private readonly AppDbContext _context;

        public AssignedAssetRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AssignedAsset>> GetAllAsync()
        {
            return await _context.AssignedAssets
                .Include(x => x.ClientAsset)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<AssignedAsset?> GetByIdAsync(int id)
        {
            return await _context.AssignedAssets
                .Include(x => x.ClientAsset)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(AssignedAsset entity)
        {
            await _context.AssignedAssets.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(AssignedAsset entity)
        {
            try
            {
                _context.AssignedAssets.Update(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

