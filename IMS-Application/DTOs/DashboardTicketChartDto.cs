using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.DTOs
{
    public class TicketChartDto
    {
        public string Day { get; set; } = string.Empty;

        public int Opened { get; set; }

        public int Solved { get; set; }
    }
}
