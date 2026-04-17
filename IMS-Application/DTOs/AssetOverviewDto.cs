namespace IMS_Application.DTOs
{
    public class AssetOverviewDto
    {
        public int Id { get; set; }
        public string? Image { get; set; }
        public string ItemName { get; set; } = null!;
        public string Status { get; set; } = null!;

        public string Brand { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string SubCategory { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string SerialNo { get; set; } = null!;
        public string Condition { get; set; } = null!;

        public string Vendor { get; set; } = null!;
        public DateTime PurchaseDate { get; set; }
        public decimal PurchaseCost { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public DateTime? WarrantyExpiry { get; set; }
        public DateTime? AmcExpiry { get; set; }

        public string? Notes { get; set; }

        public List<ChildAssetDto> Children { get; set; } = new();
    }
}
