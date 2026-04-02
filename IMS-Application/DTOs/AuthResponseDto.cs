using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.DTOs
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
