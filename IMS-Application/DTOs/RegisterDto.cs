namespace IMS_Application.DTOs
{
    public class RegisterDto
    {
        public string FullName { get; set; } = null!;
        public int DepartmentId { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
        public int CreatedBy { get; set; }
    }
}
