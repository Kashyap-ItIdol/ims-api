using AutoMapper;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace IMS_Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AuthService> _logger;
        private readonly IMapper _mapper;
        private const string OTP_KEY_PREFIX = "otp_";

        public AuthService(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IEmailService emailService,
            IMemoryCache cache,
            ILogger<AuthService> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _emailService = emailService;
            _cache = cache;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            try
            {
                var email = dto.Email.Trim().ToLower();
                var user = await _unitOfWork.Users.GetByEmailAsync(email);
                if (user == null)
                    return Result<AuthResponseDto>.Failure(ErrorMessages.InvalidCredentials, 401);

                var passwordMatches = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
                if (!passwordMatches)
                    return Result<AuthResponseDto>.Failure(ErrorMessages.InvalidPassword, 401);

                var responseDto = AttachTokensToUser(user, dto.RememberMe);
                await _unitOfWork.SaveChangesAsync();
                responseDto.User = _mapper.Map<UserInfoDto>(user);
                return Result<AuthResponseDto>.Success(responseDto, SuccessMessages.LoginSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for {Email}", dto.Email);
                return Result<AuthResponseDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<UserInfoDto>> RegisterAsync(RegisterDto dto)
        {
            try
            {
                var email = dto.Email.Trim().ToLower();
                _logger.LogInformation("Registration attempt for {Email}", email);

                if (await _unitOfWork.Users.UserExistsAsync(email))
                    return Result<UserInfoDto>.Failure(ErrorMessages.UserAlreadyExists, 409);

                var user = new User
                {
                    Email = email,
                    FullName = dto.FullName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    RoleId = 3,
                    DepartmentId = dto.DepartmentId,
                    IsVerified = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    RefreshTokens = new List<RefreshToken>()
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
                var userInfo = _mapper.Map<UserInfoDto>(user);
                return Result<UserInfoDto>.Success(userInfo, SuccessMessages.RegisterSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for {Email}", dto.Email);
                return Result<UserInfoDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var user = await _unitOfWork.Users.GetUserByRefreshTokenAsync(refreshToken);
                if (user == null)
                    return Result<AuthResponseDto>.Failure(ErrorMessages.InvalidRefreshToken, 401);

                var existingToken = user.RefreshTokens.First(rt => rt.Token == refreshToken);
                if (existingToken.IsRevoked || existingToken.Expires < DateTime.UtcNow)
                    return Result<AuthResponseDto>.Failure(ErrorMessages.InvalidOrExpiredToken, 401);

                existingToken.IsRevoked = true;
                var responseDto = AttachTokensToUser(user, rememberMe: true);
                await _unitOfWork.SaveChangesAsync();
                responseDto.User = _mapper.Map<UserInfoDto>(user);
                return Result<AuthResponseDto>.Success(responseDto, SuccessMessages.TokenRefreshSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token refresh");
                return Result<AuthResponseDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        private AuthResponseDto AttachTokensToUser(User user, bool rememberMe)
        {
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshTokenString = _tokenService.GenerateRefreshToken();
            var expirationDays = rememberMe ? 30 : 1;

            var refreshToken = new RefreshToken
            {
                Token = refreshTokenString,
                Expires = DateTime.UtcNow.AddDays(expirationDays),
                IsRevoked = false
            };

            user.RefreshTokens ??= new List<RefreshToken>();
            user.RefreshTokens.Add(refreshToken);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString
            };
        }

        public async Task<Result<bool>> LogoutAsync(string refreshToken)
        {
            try
            {
                var user = await _unitOfWork.Users.GetUserByRefreshTokenAsync(refreshToken);
                if (user != null)
                {
                    var tokenEntity = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);
                    if (tokenEntity != null && !tokenEntity.IsRevoked)
                    {
                        tokenEntity.IsRevoked = true;
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
                return Result<bool>.Success(true, SuccessMessages.LogoutSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return Result<bool>.Failure("An error occurred during logout", 500);
            }
        }

        public async Task<Result<bool>> RequestForgotPasswordAsync(ForgotPasswordRequestDto dto)
        {
            try
            {
                var email = dto.Email.Trim().ToLower();
                var user = await _unitOfWork.Users.GetByEmailAsync(email);

                if (user == null)
                    return Result<bool>.Failure(ErrorMessages.ForgotPasswordUserNotFound, 404);

                var otp = new Random().Next(1000, 9999);
                var key = $"{OTP_KEY_PREFIX}{email}";
                _cache.Set(key, otp, TimeSpan.FromMinutes(10));

                var emailResult = await _emailService.SendOtpAsync(email, otp);
                if (!emailResult.IsSuccess)
                    return Result<bool>.Failure(ErrorMessages.OtpSendFailed, 500);

                return Result<bool>.Success(true, SuccessMessages.OtpSentSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP to {Email}", dto.Email);
                return Result<bool>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<string>> VerifyOtpAsync(OtpVerificationRequestDto dto)
        {
            try
            {
                var email = dto.Email.Trim().ToLower();
                var user = await _unitOfWork.Users.GetByEmailAsync(email);

                if (user == null)
                    return Result<string>.Failure(ErrorMessages.ResetPasswordUserNotFound, 404);

                var key = $"{OTP_KEY_PREFIX}{email}";
                if (!_cache.TryGetValue(key, out int storedOtp) || storedOtp != dto.Otp)
                    return Result<string>.Failure(ErrorMessages.InvalidOrExpiredOtp, 400);

                _cache.Remove(key);
                var resetToken = _tokenService.GenerateResetToken(user.Id);
                return Result<string>.Success(resetToken, SuccessMessages.OtpVerifiedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for {Email}", dto.Email);
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            try
            {
                var userId = _tokenService.ValidateResetToken(dto.ResetToken);
                if (userId == null)
                    return Result<bool>.Failure(ErrorMessages.InvalidResetToken, 400);

                var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
                if (user == null)
                    return Result<bool>.Failure(ErrorMessages.ResetPasswordUserNotFound, 404);

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                await _unitOfWork.SaveChangesAsync();
                return Result<bool>.Success(true, SuccessMessages.PasswordResetSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return Result<bool>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }
    }
}