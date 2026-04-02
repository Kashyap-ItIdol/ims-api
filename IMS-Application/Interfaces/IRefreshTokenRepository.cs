using IMS_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetAsync(string token);
        Task AddAsync(RefreshToken refreshToken);
        Task SaveAsync();
    }
}
