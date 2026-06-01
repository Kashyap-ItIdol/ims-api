namespace IMS_Application.DTOs
{
    public class ClientAssetResponseDto
    {
        public int Id { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SubCategoryName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ClientPOC { get; set; } = string.Empty;
        public string SalesPOC { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int DeskNumber { get; set; }
        public int? UserId { get; set; }
        public string Department { get; set; } = string.Empty;

        public DateTime? ExpectedReturnDate { get; set; }

        public DateTime? ActualReturnDate { get; set; }

        public string AssignedUserName { get; set; } = string.Empty;

        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        public DateTime? AssignedDate { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }

    }
}

