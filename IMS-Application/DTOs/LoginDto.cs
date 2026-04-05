using System.ComponentModel;

namespace IMS_Application.DTOs
{
    public class LoginDto
    {
        public required string EmailAddress { get; set; }
        public required string Password { get; set; }
        [DefaultValue(false)]
        public bool RememberMe { get; set; } = false;
    }
}
