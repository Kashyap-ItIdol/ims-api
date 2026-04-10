using System;
using System.Collections.Generic;
using System.Text;
using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces
{
    public interface IUserService
    {
        Task CreateUserAsync(CreateUserDto dto, int currentUserId);
        Task UpdateUserAsync(UpdateUserDto dto, int currentUserId);
        Task<List<UserResponseDto>> GetAllUsersAsync();
        Task DeleteUserAsync(int id, int currentUserId);
    }
}
