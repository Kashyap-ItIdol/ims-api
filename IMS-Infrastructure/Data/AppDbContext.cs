using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace IMS_Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Roles> Roles { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Roles>().HasData(
                new Roles { Id = 1, Name = "Admin" },
                new Roles { Id = 2, Name = "Employee" },
                new Roles { Id = 3, Name = "Support Engineer" }
            );

            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "Backend Developer" },
                new Department { Id = 2, Name = "Frontend Developer" },
                new Department { Id = 3, Name = "Product Manager" },
                new Department { Id = 4, Name = "QA Engineer" },
                new Department { Id = 5, Name = "BA" },
                new Department { Id = 6, Name = "DevOps Engineer" },
                new Department { Id = 7, Name = "UI/UX Designer" },
                new Department { Id = 8, Name = "Mobile App Developer" }
            );

            var adminPassword = "Admin@123";
            var hashBytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(adminPassword));
            var passwordHash = Convert.ToBase64String(hashBytes);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "System Administrator",
                    Email = "admin@example.com",
                    PasswordHash = passwordHash,
                    RoleId = 1,
                    DeptId = 1,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) // fixed date to avoid migration issues
                }
            );
        }

    }
}
