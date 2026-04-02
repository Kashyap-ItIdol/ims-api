using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace IMS_Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshRepo;
        private readonly ITokenService _tokenService;

        public AuthService(
            IUserRepository userRepo,
            IRefreshTokenRepository refreshRepo,
            ITokenService tokenService)
        {
            _userRepo = userRepo;
            _refreshRepo = refreshRepo;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepo.GetByEmailAsync(dto.Username);

            if (user == null || user.PasswordHash != Hash(dto.Password))
                throw new Exception("Invalid credentials");

            return await GenerateTokens(user);
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userRepo.CheckUserExixst(dto.Email);

            if (existingUser)
                throw new Exception("User already exists");

            var user = new User
            {
                Email = dto.Email,
                FullName = dto.FullName,
                PasswordHash = Hash(dto.Password),
                RoleId = dto.RoleId,
                DepartmentId = dto.DeptId,
                IsActive = true,
                CreatedBy = dto.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(user);

            user = await _userRepo.GetByEmailAsync(user.Email);

            return await GenerateTokens(user);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var token = await _refreshRepo.GetAsync(refreshToken);

            if (token == null || token.IsRevoked || token.Expires < DateTime.UtcNow)
                throw new Exception("Invalid refresh token");

            token.IsRevoked = true;
            await _refreshRepo.SaveAsync();

            var user = await _userRepo.GetByEmailAsync(token.User.Email);

            return await GenerateTokens(user);
        }

        private async Task<AuthResponseDto> GenerateTokens(User user)
        {
            var accessToken = _tokenService.GenerateAccessToken(user);

            var refreshToken = new RefreshToken
            {
                Token = _tokenService.GenerateRefreshToken(),
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = user.Id,
                IsRevoked = false
            };

            await _refreshRepo.AddAsync(refreshToken);
            await _refreshRepo.SaveAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        private string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(sha.ComputeHash(bytes));
        }
    }
}
