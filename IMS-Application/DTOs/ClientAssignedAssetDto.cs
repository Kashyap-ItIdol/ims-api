using System;

namespace IMS_Application.DTOs
{
    public class ClientAssignedAssetDto
    {
        public int Id { get; set; }

        public int? ClientAssetId { get; set; }

        public string? ClientAssetName { get; set; }

        public required string ItemName { get; set; }

        public required string SerialNumber { get; set; }

        public DateTime AssignedDate { get; set; }

        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

        public bool IsDeleted { get; set; }
    }
}