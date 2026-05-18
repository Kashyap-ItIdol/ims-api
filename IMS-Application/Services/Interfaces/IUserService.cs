using IMS_Application.Common.Models;
using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<Result<string>> CreateUserAsync(CreateUserDto dto, int currentUserId);
        Task<Result<string>> UpdateUserAsync(UpdateUserDto dto, int currentUserId);
        Task<Result<string>> DeleteUserAsync(int id, int currentUserId);
        Task<Result<List<UserResponseDto>>> GetAllUsersAsync();
        Task<Result<UserOverviewResponseDto>> GetUserOverviewByIdAsync(int id);
        Task<Result<List<UserResponseDto>>> FilterUsersAsync(UserFilterDto filter);
        Task<Result<List<UserResponseDto>>> SearchUsersAsync(string query);
        Task<Result<UserFilterOptionsDto>> GetUserFilterOptionsAsync();
    }
}
