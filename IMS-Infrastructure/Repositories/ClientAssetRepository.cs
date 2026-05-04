using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Repositories
{
    public class ClientAssetRepository : Repository<ClientAsset>, IClientAssetRepository
    {
        public ClientAssetRepository(AppDbContext context) : base(context)
        {
        }

        public new async Task<ClientAsset?> GetByIdAsync(int id)
        {
            return await _context.ClientAssets
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .Include(x => x.AssignedUser)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<ClientAsset?> GetWithAttachmentsAsync(int id)
        {
            return await _context.ClientAssets
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .Include(x => x.AssignedUser)
                .Include(x => x.Attachments)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ClientAssets
                .AnyAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<bool> UpdateAsync(ClientAsset asset)
        {
            try
            {
                base.Update(asset);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var asset = await GetByIdAsync(id);
            if (asset == null)
                return false;

            asset.IsDeleted = true;
            asset.DeletedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ClientAsset>> FilterAsync(ClientAssetFilterDto filter)
        {
            var query = _context.ClientAssets
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .Include(x => x.AssignedUser)
                .Where(x => !x.IsDeleted)
                .AsQueryable();

            if (filter.Status != null && filter.Status.Count > 0)
            {
                query = query.Where(x => filter.Status.Contains(x.Status));
            }
            if (filter.Brand != null && filter.Brand.Count > 0)
            {
                query = query.Where(x => filter.Brand.Contains(x.Brand));
            }

            if (filter.ClientProject != null && filter.ClientProject.Count > 0)
            {
                query = query.Where(x => filter.ClientProject.Contains(x.ClientName));
            }
 
            if (filter.AssignedTo != null && filter.AssignedTo.Count > 0)
            {
                if (filter.AssignedTo.Contains(0))
                {
                    query = query.Where(x => x.AssignedTo == null);
                }
                else
                {
                    query = query.Where(x => x.AssignedTo.HasValue && 
                                          filter.AssignedTo.Contains(x.AssignedTo.Value));
                }
            }
            
            if (filter.AssignedFrom.HasValue)
            {
                query = query.Where(x => x.AssignedDate.HasValue && 
                                      x.AssignedDate.Value >= filter.AssignedFrom.Value);
            }

            if (!string.IsNullOrEmpty(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(x =>
                    x.AssetName.ToLower().Contains(search) ||
                    x.SerialNumber.ToLower().Contains(search) ||
                    x.Model.ToLower().Contains(search));
            }

            var result = await query.ToListAsync();
            return result;
        }

        public async Task AddAttachmentAsync(ClientAssetAttachment attachment)
        {
            await _context.ClientAssetAttachments.AddAsync(attachment);
        }

        public async Task<ClientAssetAttachment?> GetAttachmentByIdAsync(int id)
        {
            return await _context.ClientAssetAttachments
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<IEnumerable<ClientAssetAttachment>> GetAttachmentsByAssetIdAsync(int assetId)
        {
            return await _context.ClientAssetAttachments
                .Where(x => x.ClientAssetId == assetId && !x.IsDeleted)
                .ToListAsync();
        }

        public async Task DeleteAttachmentAsync(ClientAssetAttachment attachment)
        {
            attachment.IsDeleted = true;
            attachment.DeletedAt = DateTime.UtcNow;
            
            _context.ClientAssetAttachments.Update(attachment);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}