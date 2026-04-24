using AutoMapper;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

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
            ILogger<AuthService> logger, IMapper mapper)
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
                //  Generate Tokens AND attach the RefreshToken to the User object in memory
                var responseDto = AttachTokensToUser(user, dto.RememberMe);
                await _unitOfWork.SaveChangesAsync();
                // Map the User Info (This will safely use the AutoMapper Flattening fix we added earlier!)
                responseDto.User = _mapper.Map<UserInfoDto>(user);
                return Result<AuthResponseDto>.Success(responseDto, SuccessMessages.LoginSuccess);
            }
            catch (OperationCanceledException)
            {
                throw;
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
                // Check if user exists using the UoW
                if (await _unitOfWork.Users.UserExistsAsync(email))
                    return Result<UserInfoDto>.Failure(ErrorMessages.UserAlreadyExists, 409);
                //  user manual mapping  
                var user = new User
                {
                    Email = email,
                    FullName = dto.FullName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    RoleId = 3,
                    DepartmentId = dto.DepartmentId,
                    IsActive = true,
                    IsVerified = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    RefreshTokens = new List<RefreshToken>()
                };

                //  Add the User (and its attached RefreshToken) to EF Core's memory tracking
                await _unitOfWork.Users.AddAsync(user);
                // Commit EVERYTHING to the database in ONE atomic transaction
                await _unitOfWork.SaveChangesAsync();
                //  Map the final user data to the response
                var userInfo = _mapper.Map<UserInfoDto>(user);
                return Result<UserInfoDto>.Success(userInfo, SuccessMessages.RegisterSuccess);
            }
            catch (OperationCanceledException)
            {
                throw;
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
                // Get the User and ALL their tokens in one query
                var user = await _unitOfWork.Users.GetUserByRefreshTokenAsync(refreshToken);
                if (user == null)
                    return Result<AuthResponseDto>.Failure(ErrorMessages.InvalidRefreshToken, 401);
                // Find the exact token the user passed in
                var existingToken = user.RefreshTokens.First(rt => rt.Token == refreshToken);
                // Security Validation
                if (existingToken.IsRevoked || existingToken.Expires < DateTime.UtcNow)
                    return Result<AuthResponseDto>.Failure(ErrorMessages.InvalidOrExpiredToken, 401);
                // Refresh Token Rotation (CRITICAL SECURITY)
                // We revoke the token they just used so it can never be used again.
                existingToken.IsRevoked = true;
                // Generate NEW tokens and attach to User (Reusing our helper!)
                // We pass 'true' to ensure they get another 30-day token.
                var responseDto = AttachTokensToUser(user, rememberMe: true);
                // Commit everything (Revoked status AND new token) in ONE transaction
                await _unitOfWork.SaveChangesAsync();
                responseDto.User = _mapper.Map<UserInfoDto>(user);
                return Result<AuthResponseDto>.Success(responseDto, SuccessMessages.TokenRefreshSuccess);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token refresh");
                return Result<AuthResponseDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        private AuthResponseDto AttachTokensToUser(User user, bool rememberMe)
        {
            // Generate the raw strings
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshTokenString = _tokenService.GenerateRefreshToken();
            var expirationDays = rememberMe ? 30 : 1;
            // Create the token entity
            var refreshToken = new RefreshToken
            {
                Token = refreshTokenString,
                Expires = DateTime.UtcNow.AddDays(expirationDays),
                IsRevoked = false
            };
            // Attach the token to the user object
            user.RefreshTokens ??= new List<RefreshToken>();
            user.RefreshTokens.Add(refreshToken);
            // Return the DTO structure (User mapping happens later)
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
                // Find the user holding this token
                var user = await _unitOfWork.Users.GetUserByRefreshTokenAsync(refreshToken);
                if (user != null)
                {
                    // Find the exact token
                    var tokenEntity = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);
                    // If it exists and isn't already revoked, revoke it!
                    if (tokenEntity != null && !tokenEntity.IsRevoked)
                    {
                        tokenEntity.IsRevoked = true;
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
                // We always return success, even if the token was already dead, 
                // because the end result is what the user wanted: they are logged out.
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
                // Generate reset token - short lived JWT for reset
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
