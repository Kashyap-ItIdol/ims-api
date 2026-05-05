namespace IMS_Application.DTOs
{
    public class OtpVerificationRequestDto
    {
        public string Email { get; set; } = null!;
        public int Otp { get; set; }
    }
}
