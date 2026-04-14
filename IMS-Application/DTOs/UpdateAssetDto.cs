namespace IMS_Application.DTOs
{
    public class UpdateAssetDto
    {
        public int Id { get; set; }   // asset being edited

        public string? Image { get; set; }
        public required string ItemName { get; set; }
        public required int StatusId { get; set; }
        public required int CategoryId { get; set; }
        public required int SubCategoryId { get; set; }
        public required int ConditionId { get; set; }
        public required string Brand { get; set; }
        public required string Model { get; set; }
        public required string SerialNo { get; set; }

        public string Vendor { get; set; } = null!;
        public decimal PurchaseCost { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public DateTime? WarrantyExpiry { get; set; }
        public DateTime? AmcExpiry { get; set; }

        public int? AssignedTo { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }

        public string? Location { get; set; }
        public string? TableNo { get; set; }
    }
}