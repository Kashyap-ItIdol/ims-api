using IMS_API.Controllers.Base;
using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(
            IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto) =>
            FromResult(await _authService.LoginAsync(dto));

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto) =>
            FromResult(await _authService.RegisterAsync(dto));

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequestDto dto) =>
            FromResult(await _authService.RefreshTokenAsync(dto.RefreshToken));

    }
}
