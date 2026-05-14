using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Repositories
{
    public class EmailTemplateRepository : Repository<EmailTemplate>, IEmailTemplateRepository
    {
        public EmailTemplateRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<EmailTemplate?> GetByNameAsync(string name)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == name && x.IsActive);
        }
        public async Task<EmailTemplate?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public new async Task<IEnumerable<EmailTemplate>> GetAllAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync();
        }

        public async Task<EmailTemplate> AddAsync(EmailTemplate template)
        {
            template.UpdatedAt = DateTime.UtcNow;
            await _dbSet.AddAsync(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task UpdateAsync(EmailTemplate template)
        {
            template.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(template);
            await _context.SaveChangesAsync();
        }
    }
}
