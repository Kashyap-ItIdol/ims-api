using System;

namespace IMS_Application.DTOs
{
    public class UpdateClientAssignedAssetDto
    {
        public int? ClientAssetId { get; set; }

        public required string ItemName { get; set; }

        public required string SerialNumber { get; set; }

        public DateTime AssignedDate { get; set; }
    }
}