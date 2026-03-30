using IMS_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByUsernameAsync(string username);
        Task AddAsync(User user);
    }
}
