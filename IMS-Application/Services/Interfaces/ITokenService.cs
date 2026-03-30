using IMS_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
