namespace IMS_Application.DTOs
{
    public class CreateChildAssetDto
    {
        public int ParentId { get; set; }

        public string ItemName { get; set; } = null!;
        public int StatusId { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int ConditionId { get; set; }
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string SerialNo { get; set; } = null!;

        public string Vendor { get; set; } = null!;
        public decimal PurchaseCost { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string InvoiceNumber { get; set; } = null!;
    }
}
