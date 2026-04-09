using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.DTOs
{
    public class AddAssetDto
    {
        
        public List<AssetItemDto> Assets { get; set; } = new();

        
        public int? AssignedTo { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }

        public string? Location { get; set; }
        public string? TableNo { get; set; }
    }
}
