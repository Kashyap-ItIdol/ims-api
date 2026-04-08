using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Security.Cryptography;
using System.Text;

namespace IMS_Infrastructure.Data.Configurations
{
    public class AdminDataSeedConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            var adminPassword = "Admin@123";
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(adminPassword));
            var passwordHash = Convert.ToBase64String(hashBytes);

            builder.HasData(
                new User
                {
                    Id = 1,
                    FullName = "System Administrator",
                    Email = "admin@example.com",
                    PasswordHash = "6G94qKPK8LYNjnTllCqm2G3BUM08AzOK7yW30tfjrMc=",
                    RoleId = 1,
                    DepartmentId = 1,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
