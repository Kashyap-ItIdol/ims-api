using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IMS_Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }
        public string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role.Name),
                new Claim("userId", user.Id.ToString())
            };
            return GenerateJwtToken(claims, 15); // 15 min
        }
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        public string GenerateResetToken(int userId)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("purpose", "password_reset")
            };
            return GenerateJwtToken(claims, 15); // 15 min
        }
        public int? ValidateResetToken(string token)
        {
            try
            {
                var jwtKey = _config["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;
                var purpose = jwtToken.Claims.FirstOrDefault(x => x.Type == "purpose")?.Value;
                if (purpose != "password_reset")
                    return null;
                return int.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            }
            catch
            {
                return null;
            }
        }
        private string GenerateJwtToken(Claim[] claims, int expiryMinutes)
        {
            var jwtKey = _config["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
