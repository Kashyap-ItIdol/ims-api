using IMS_Application.Common.Models;
using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto);
        Task<Result<UserInfoDto>> RegisterAsync(RegisterDto dto);
        Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken);
        Task<Result<bool>> LogoutAsync(string refreshToken);
    }
}
