using IMS_API.Controllers.Base;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (result.IsSuccess)
            {
                // 30 days for "Remember Me"
                int? expireDays = dto.RememberMe ? 30 : null;

                SetRefreshTokenCookie(result.Data!.RefreshToken, expireDays);
            }

            return FromResult(result);
        }

        // Only Admins can hit this endpoint
        //[Authorize(Roles = "Admin")]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            // Optional but highly recommended: 
            // Extract the Admin's ID directly from their JWT token to prevent spoofing
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            dto.CreatedBy = adminId;

            return FromResult(await _authService.RegisterAsync(dto));
        }

        [HttpPost("refreshToken")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken()
        {
            // Extract the token from the secure cookie rather than the DTO
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { success = false, message = ErrorMessages.NoRefreshToken });

            // 3. Service handles the heavy database and token logic
            var result = await _authService.RefreshTokenAsync(refreshToken);

            // 4. Controller handles the HTTP response injection
            if (result.IsSuccess)
            {
                SetRefreshTokenCookie(result.Data!.RefreshToken);
            }

            return FromResult(result);
        }

        [HttpPost("logout")]
        [AllowAnonymous] // Allow anonymous so even if their access token expired, they can still clear their cookie
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                //  Revoke in the database
                await _authService.LogoutAsync(refreshToken);

                // Delete the HTTP-only cookie from the user's browser
                DeleteRefreshTokenCookie();
            }

            return FromResult(Result<bool>.Success(true, SuccessMessages.LogoutSuccess));
        }
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto dto)
        {
            var result = await _authService.RequestForgotPasswordAsync(dto);
            return FromResult(result);
        }
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp(OtpVerificationRequestDto dto)
        {
            var result = await _authService.VerifyOtpAsync(dto);
            return FromResult(result);
        }
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);
            return FromResult(result);
        }
    }
}
