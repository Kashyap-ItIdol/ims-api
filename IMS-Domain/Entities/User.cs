namespace IMS_Domain.Entities
{
    public class User : BaseEntity
    {
     //   public int Id { get; set; }

        public string FullName { get; set; } = null!;

        public required string Email { get; set; }

        public required string PasswordHash { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public string? PasswordResetToken { get; set; }

        public DateTime? ResetTokenExpires { get; set; }

        public string? ProfileImg { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsVerified { get; set; } = false;

       

        // Navigation
        public ICollection<InventoryAssignment> InventoryAssignments { get; set; } = new List<InventoryAssignment>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}