using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.DTOs
{
    public class CreateUserDto
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required int RoleId { get; set; }
        public int? DepartmentId { get; set; }
        public string Password { get; set; }
        public string? ProfileImg { get; set; }
    }
}
