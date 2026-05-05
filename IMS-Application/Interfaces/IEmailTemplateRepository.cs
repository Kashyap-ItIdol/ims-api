using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface IEmailTemplateRepository
    {
        Task<EmailTemplate?> GetByNameAsync(string name);
        Task<EmailTemplate?> GetByIdAsync(int id);
        Task<IEnumerable<EmailTemplate>> GetAllAsync();
        Task<EmailTemplate> AddAsync(EmailTemplate template);
        Task UpdateAsync(EmailTemplate template);
    }
}
