using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.DTOs
{

    public class AssignAssetDto
    {
        public int AssetId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public string? Location { get; set; }
        public string? TableNo { get; set; }
    }
}
