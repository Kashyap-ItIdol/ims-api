using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Repositories
{
    public class EmailTemplateRepository : IEmailTemplateRepository
    {
        private readonly AppDbContext _context;

        public EmailTemplateRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EmailTemplate?> GetByNameAsync(string name)
        {
            return await _context.EmailTemplates
                .FirstOrDefaultAsync(x => x.Name == name && x.IsActive);
        }

        public async Task<EmailTemplate?> GetByIdAsync(int id)
        {
            return await _context.EmailTemplates.FindAsync(id);
        }

        public async Task<IEnumerable<EmailTemplate>> GetAllAsync()
        {
            return await _context.EmailTemplates
                .Where(x => x.IsActive)
                .ToListAsync();
        }

        public async Task<EmailTemplate> AddAsync(EmailTemplate template)
        {
            _context.EmailTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task UpdateAsync(EmailTemplate template)
        {
            template.UpdatedAt = DateTime.UtcNow;
            _context.EmailTemplates.Update(template);
            await _context.SaveChangesAsync();
        }
    }
}
