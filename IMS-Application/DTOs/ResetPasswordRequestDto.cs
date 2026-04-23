namespace IMS_Application.DTOs
{
    public class ResetPasswordRequestDto
    {
        public string ResetToken { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }
}
