namespace IMS_Application.DTOs
{
    public class UpdateAssetDto
    {
        public int Id { get; set; }

        // basic fields
        public string? Image { get; set; }
        public string ItemName { get; set; } = null!;
        public int StatusId { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int ConditionId { get; set; }
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string SerialNo { get; set; } = null!;

        // purchase
        public string Vendor { get; set; } = null!;
        public decimal PurchaseCost { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public DateTime? WarrantyExpiry { get; set; }
        public DateTime? AmcExpiry { get; set; }

        // assignment
        public int? AssignedTo { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }

        // 🔥 important flags
        public bool IsFromParentContext { get; set; }
        public bool IsManualUnlink { get; set; }
    }
}
