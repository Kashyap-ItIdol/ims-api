using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task<bool> CheckUserExixst(string email);
        Task<User?> GetByIdAsync(int id);
    }
}
