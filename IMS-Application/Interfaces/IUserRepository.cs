﻿using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> UserExistsAsync(string email);
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
        Task<bool> ExistsAsync(int userId);
        Task<bool> TableAlreadyAssignedAsync(string tableNo);
        Task<List<User>> GetAllWithRolesAsync();
        Task<List<User>> GetUsersWithOpenTicketsAsync();
        Task<List<User>> SearchAsync(string query);
    }
}