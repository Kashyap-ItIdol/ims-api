using IMS_Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    }
}
