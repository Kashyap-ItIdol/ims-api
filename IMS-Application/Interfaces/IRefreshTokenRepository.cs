using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetAsync(string token);
        Task AddAsync(RefreshToken refreshToken);
        Task SaveAsync();
    }
}
