using AutoMapper;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace IMS_Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshRepo;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;
        private readonly IMapper _mapper;

        public AuthService(
            IUserRepository userRepo,
            IRefreshTokenRepository refreshRepo,
            ITokenService tokenService,
            ILogger<AuthService> logger, IMapper mapper)
        {
            _userRepo = userRepo;
            _refreshRepo = refreshRepo;
            _tokenService = tokenService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            try
            {
                var email = dto.EmailAddress.Trim().ToLower();

                var user = await _userRepo.GetByEmailAsync(email);

                if (user == null)
                    return Result<AuthResponseDto>.Failure(ErrorMessages.InvalidCredentials, 401);

                var passwordMatches = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

                if (!passwordMatches)
                    return Result<AuthResponseDto>.Failure(ErrorMessages.InvalidPassword, 401);

                var tokens = await GenerateTokens(user);

                var response = new AuthResponseDto
                {
                    AccessToken = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken,
                    User = _mapper.Map<UserInfoDto>(user)
                };

                return Result<AuthResponseDto>.Success(response,SuccessMessages.LoginSuccess);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for {Email}", dto.EmailAddress);

                return Result<AuthResponseDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto)
        {
            try
            {
                var email = dto.Email.Trim().ToLower();

                _logger.LogInformation("Registration attempt for {Email}", email);

                // Check if user already exists
                var existingUser = await _userRepo.UserExistsAsync(email);
                if (existingUser)
                    return Result<AuthResponseDto>.Failure(ErrorMessages.UserAlreadyExists, 409);

                // Create user manual mapping  
                var user = new User
                {
                    Email = email,
                    FullName = dto.FullName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),

                    RoleId = dto.RoleId,
                    DepartmentId = dto.DeptId,

                    IsActive = true,
                    IsVerified = false,
                    IsDeleted = false,

                    CreatedBy = dto.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                };

                // Save user
                await _userRepo.AddAsync(user);

                // IMPORTANT: reload with navigation properties (Role, Department)
                var createdUser = await _userRepo.GetByEmailAsync(email);

                if (createdUser == null)
                {
                    _logger.LogError("User created but could not be retrieved: {Email}", email);

                    return Result<AuthResponseDto>.Failure(
                        ErrorMessages.UnexpectedError,
                        500);
                }

                // Generate tokens
                var tokens = await GenerateTokens(createdUser);

                // Build response
                var response = new AuthResponseDto
                {
                    AccessToken = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken,
                    User = _mapper.Map<UserInfoDto>(createdUser)
                };

                return Result<AuthResponseDto>.Success(response,SuccessMessages.RegisterSuccess);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for {Email}", dto.Email);

                return Result<AuthResponseDto>.Failure(
                    ErrorMessages.UnexpectedError,
                    500);
            }
        }

        public async Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return Result<AuthResponseDto>.Failure("Refresh token is required.", 400);

                var token = await _refreshRepo.GetAsync(refreshToken);

                if (token == null || token.IsRevoked || token.Expires < DateTime.UtcNow)
                    return Result<AuthResponseDto>.Failure("Invalid or expired refresh token.", 401);

                token.IsRevoked = true;
                await _refreshRepo.SaveAsync();

                var user = await _userRepo.GetByEmailAsync(token.User.Email);
                if (user == null)
                    return Result<AuthResponseDto>.Failure("Invalid or expired refresh token.", 401);

                var tokens = await GenerateTokens(user);
                return Result<AuthResponseDto>.Success(tokens);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token refresh");
                return Result<AuthResponseDto>.Failure(
                    "An unexpected error occurred. Please try again later.",
                    500);
            }
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

        //private string Hash(string password)
        //{
        //    using var sha = SHA256.Create();
        //    var bytes = Encoding.UTF8.GetBytes(password);
        //    return Convert.ToBase64String(sha.ComputeHash(bytes));
        //}
    }
}
