using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.DTOs
{
    public class InventoryAssignmentDto
    {
        public int? UserId { get; set; }

        public DateTime AssignedDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }

        public string Location { get; set; } = string.Empty;
        public string DeskNumber { get; set; } = string.Empty;
    }
}
