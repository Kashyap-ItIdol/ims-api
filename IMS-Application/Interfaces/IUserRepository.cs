using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> UserExistsAsync(string email);
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
        Task<Dictionary<int, User>> GetUsersByIdsAsync(IEnumerable<int> ids);
    }
}
