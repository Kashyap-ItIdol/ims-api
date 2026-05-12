using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Repositories;

public class AssetAssignmentRepository : Repository<AssetAssignment>, IAssetAssignmentRepository
{
    public AssetAssignmentRepository(AppDbContext context) : base(context)
    {
    }

    public new async Task<AssetAssignment> AddAsync(AssetAssignment entity)
    {
        await base.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public new async Task<IEnumerable<AssetAssignment>> GetAllAsync()
    {
        return await _context.AssetAssignments
            .Where(x => !x.IsDeleted)
            .ToListAsync();
    }

    public new async Task<AssetAssignment?> GetByIdAsync(int id)
    {
        return await _context.AssetAssignments
            .Where(x => !x.IsDeleted)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task UpdateAsync(AssetAssignment entity)
    {
        base.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = 1; 
            
            await _context.SaveChangesAsync();
        }
    }
}
