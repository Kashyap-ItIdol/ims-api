using System.ComponentModel.DataAnnotations;

namespace IMS_Domain.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string FullName { get; set; } = null!;

        public required string Email { get; set; }

        public required string PasswordHash { get; set; }

        public int DeptId { get; set; }

        public int RoleId { get; set; }

        public string? PasswordResetToken { get; set; }

        public DateTime? ResetTokenExpires { get; set; }

        public string? ProfileImg { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DeletedDate { get; set; }

        public int? DeletedBy { get; set; }

        // Navigation
        public Roles Role { get; set; } = null!;
        public Department Department { get; set; } = null!;
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();


    }

}
