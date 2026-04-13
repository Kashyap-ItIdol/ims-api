using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> UserExistsAsync(string email);
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);

        // for user module 

        Task<User?> GetByIdAsync(int id);
        Task<List<User>> GetAllAsync();
        Task AddAsync(User user);
        void Update(User user);
        Task SaveChangesAsync();

        Task<bool> ExistsAsync(int userId);

        Task<bool> TableAlreadyAssignedAsync(string tableNo);

      //  Task<List<User>> SearchAsync(string query);
    }
}
