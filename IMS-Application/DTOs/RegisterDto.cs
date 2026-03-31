using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.DTOs
{
    public class RegisterDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public int DeptId { get; set; }
    }
}
