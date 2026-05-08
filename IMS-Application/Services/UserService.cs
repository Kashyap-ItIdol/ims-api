using AutoMapper;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Constants;
using IMS_Domain.Entities;
using Microsoft.Extensions.Logging;


namespace IMS_Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<string>> CreateUserAsync(CreateUserDto dto, int currentUserId)
        {
            try
            {
                var email = dto.Email.Trim().ToLower();

                if (await _unitOfWork.Users.UserExistsAsync(email))
                    return Result<string>.Failure(ErrorMessages.UserAlreadyExists, 400);

                if (dto.RoleId == RoleConstants.Admin)
                    return Result<string>.Failure(ErrorMessages.CannotAssignAdminRole, 400);

                var user = _mapper.Map<User>(dto);

                user.Email = email;
                var defaultPassword = Guid.NewGuid().ToString("N")[..8];
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
                user.IsActive = true;
                user.IsDeleted = false;
                user.CreatedAt = DateTime.UtcNow;
                user.CreatedBy = currentUserId;

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success(SuccessMessages.RegisterSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<string>> UpdateUserAsync(UpdateUserDto dto, int currentUserId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(dto.Id);

                if (user == null)
                    return Result<string>.Failure(ErrorMessages.UserNotFound, 404);

                if (user.RoleId != RoleConstants.Employee &&
                    user.RoleId != RoleConstants.SupportEngineer)
                {
                    return Result<string>.Failure(ErrorMessages.OnlyEmployeeOrSupportCanUpdate, 400);
                }

                if (dto.RoleId == RoleConstants.Admin)
                    return Result<string>.Failure(ErrorMessages.CannotAssignAdminRole, 400);

                var email = dto.Email?.Trim().ToLower();
                if (!string.IsNullOrEmpty(email) && email != user.Email)
                {
                    if (await _unitOfWork.Users.UserExistsAsync(email))
                        return Result<string>.Failure(ErrorMessages.UserAlreadyExists, 400);
                }

                _mapper.Map(dto, user);

                if (!string.IsNullOrEmpty(email))
                    user.Email = email;

                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = currentUserId;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success(SuccessMessages.UserUpdatedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with id {UserId}", dto.Id);
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<List<UserResponseDto>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _unitOfWork.Users.GetAllWithRolesAsync();

                var result = users.Select((u, index) => new UserResponseDto
                {
                    Id = u.Id,
                    EmpCode = $"EMP-{(index + 1).ToString("D3")}",
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role.Name,
                    Department = u.Department != null ? u.Department.Name : null,
                    IsActive = u.IsActive
                }).ToList();

                return Result<List<UserResponseDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return Result<List<UserResponseDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<string>> DeleteUserAsync(int id, int currentUserId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);

                if (user == null)
                    return Result<string>.Failure(ErrorMessages.UserNotFound, 404);

                if (user.RoleId != RoleConstants.Employee &&
                    user.RoleId != RoleConstants.SupportEngineer)
                {
                    return Result<string>.Failure(ErrorMessages.OnlyEmployeeOrSupportCanDelete, 400);
                }

                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
                user.DeletedBy = currentUserId;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success(SuccessMessages.UserDeletedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with id {UserId}", id);
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }
    }
}

