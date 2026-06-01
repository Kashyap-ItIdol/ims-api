using System;

namespace IMS_Application.DTOs
{
    public class CreateClientAssignedAssetDto
    {
        public int? ClientAssetId { get; set; }

        public required string ItemName { get; set; }

        public required string SerialNumber { get; set; }

        public DateTime AssignedDate { get; set; }
    }
}