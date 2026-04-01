namespace IMS_Application.DTOs
{
    public class RegisterDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public int DeptId { get; set; }
        public int RoleId { get; set; }
    }
}
