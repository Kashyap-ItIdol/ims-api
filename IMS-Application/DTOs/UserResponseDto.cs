namespace IMS_Application.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string EmpCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? Department { get; set; }
        public bool IsDeleted { get; set; }
        public string Status => IsDeleted ? "Inactive" : "Active";
    }
}
