namespace IMS_Application.DTOs
{
    public class AssetResponseDto
    {
        public string? Image { get; set; }
        public string ItemName { get; set; } = default!;
        public int StatusId { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int ConditionId { get; set; }

        public string Brand { get; set; } = default!;
        public string Model { get; set; } = default!;
        public string SerialNo { get; set; } = default!;

        public string? Vendor { get; set; }
        public decimal? PurchaseCost { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string? InvoiceNumber { get; set; }

        public DateTime? WarrantyExpiry { get; set; }
        public DateTime? AmcExpiry { get; set; }

        public int? AssignedTo { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }

        public string? Location { get; set; }
        public string? TableNo { get; set; }
    }
}
