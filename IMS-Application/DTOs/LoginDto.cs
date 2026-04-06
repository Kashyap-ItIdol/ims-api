using System.ComponentModel;

namespace IMS_Application.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; } = null!;
        public  string Password { get; set; } = null!;

        [DefaultValue(false)]
        public bool RememberMe { get; set; } = false;
    }
}
