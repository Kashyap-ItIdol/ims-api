using IMS_Domain.Entities;

namespace IMS_Application.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        string GenerateResetToken(int userId);
        int? ValidateResetToken(string token);
    }
}
