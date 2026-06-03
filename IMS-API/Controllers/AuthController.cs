using IMS_API.Controllers.Base;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (result.IsSuccess)
            {
                int? expireDays = dto.RememberMe ? 30 : null;
                SetRefreshTokenCookie(result.Data!.RefreshToken, expireDays);
            }

            return FromResult(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            return FromResult(await _authService.RegisterAsync(dto));
        }

        [HttpPost("refreshToken")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { success = false, message = ErrorMessages.NoRefreshToken });

            var result = await _authService.RefreshTokenAsync(refreshToken);

            if (result.IsSuccess)
                SetRefreshTokenCookie(result.Data!.RefreshToken);

            return FromResult(result);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _authService.LogoutAsync(refreshToken);
                DeleteRefreshTokenCookie();
            }

            return FromResult(Result<bool>.Success(true, SuccessMessages.LogoutSuccess));
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto dto)
        {
            return FromResult(await _authService.RequestForgotPasswordAsync(dto));
        }

        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp(OtpVerificationRequestDto dto)
        {
            return FromResult(await _authService.VerifyOtpAsync(dto));
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto dto)
        {
            return FromResult(await _authService.ResetPasswordAsync(dto));
        }
    }
}
