namespace IMS_Application.DTOs
{
    public class CreateChildAssetDto
    {
        public int ParentId { get; set; }
        public string ItemName { get; set; }
        public int StatusId { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int ConditionId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string SerialNo { get; set; }
        public string Vendor { get; set; }
        public decimal PurchaseCost { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public DateTime? AmcExpiry { get; set; }
    }
}
