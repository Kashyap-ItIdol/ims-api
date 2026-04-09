namespace IMS_Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; } = null!;

        public required string Email { get; set; }

        public required string PasswordHash { get; set; }

        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public string? PasswordResetToken { get; set; }

        public DateTime? ResetTokenExpires { get; set; }

        public string? ProfileImg { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsVerified { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DeletedAt { get; set; }

        public int? DeletedBy { get; set; }

        // Navigation
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}