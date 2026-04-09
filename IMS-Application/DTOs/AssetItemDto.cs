namespace IMS_Application.DTOs
{
    public class AssetItemDto
    {
        public string? Image { get; set; }

        public required string ItemName { get; set; }
        public required int StatusId { get; set; }
        public required int CategoryId { get; set; }
        public required int SubCategoryId { get; set; }
        public required int ConditionId { get; set; }

        public required string Brand { get; set; }
        public required string Model { get; set; }
        public required string SerialNo { get; set; }

        public bool IsPrimary { get; set; }
        public bool IsPurchaseDetailsSame { get; set; }


        public string? Vendor { get; set; }
        public decimal? PurchaseCost { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string? InvoiceNumber { get; set; }

        public DateTime? WarrantyExpiry { get; set; }
        public DateTime? AmcExpiry { get; set; }
    }
}
