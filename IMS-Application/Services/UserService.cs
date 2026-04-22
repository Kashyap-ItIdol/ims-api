using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Constants;
using IMS_Domain.Entities;

namespace IMS_Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }


        public async Task CreateUserAsync(CreateUserDto dto, int currentUserId)
        {
            var email = dto.Email.Trim().ToLower();

            // ✅ Check duplicate email
            if (await _userRepository.UserExistsAsync(email))
                throw new Exception("User already exists");

            // ✅ Prevent creating Admin users
            if (dto.RoleId == RoleConstants.Admin)
                throw new Exception("Cannot assign Admin role");

            var user = new User
            {
                FullName = dto.FullName,
                Email = email,
                RoleId = dto.RoleId,
                DepartmentId = dto.DepartmentId,
                ProfileImg = dto.ProfileImg,

                // ✅ Correct hashing
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),

                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
        }


        public async Task UpdateUserAsync(UpdateUserDto dto, int currentUserId)
        {
            var user = await _userRepository.GetByIdAsync(dto.Id);

            if (user == null)
                throw new Exception("User not found");

            
            if (user.RoleId != RoleConstants.Employee &&
                user.RoleId != RoleConstants.SupportEngineer)
            {
                throw new Exception("Only Employee or Support Engineer can be updated");
            }

            if (dto.RoleId == RoleConstants.Admin)
                throw new Exception("Cannot assign Admin role");

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.RoleId = dto.RoleId;
            user.DepartmentId = dto.DepartmentId;
            user.ProfileImg = dto.ProfileImg;
            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = currentUserId;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
        }

        
        public async Task<List<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();

            return users.Select((u, index) => new UserResponseDto
            {
                Id = u.Id,
                EmpCode = $"EMP-{(index + 1).ToString("D3")}",
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.Name,
                Department = u.Department != null ? u.Department.Name : null,
                IsActive = u.IsActive
            }).ToList();
        }

        
        public async Task DeleteUserAsync(int id, int currentUserId)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                throw new Exception("User not found");

            
            if (user.RoleId != RoleConstants.Employee &&
                user.RoleId != RoleConstants.SupportEngineer)
            {
                throw new Exception("Only Employee or Support Engineer can be deleted");
            }

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.DeletedBy = currentUserId;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
        }
    }
}
