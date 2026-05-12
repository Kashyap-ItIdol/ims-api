namespace IMS_Application.DTOs
{
    public class CreateAssetDto
    {
        public string AssetName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty; 

        public int CategoryId { get; set; }

        public int SubCategoryId { get; set; }

        public string Brand { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public string SerialNumber { get; set; } = string.Empty;

        public int ConditionId { get; set; }

        public string ClientPOC { get; set; } = string.Empty;

        public string SalesPOC { get; set; } = string.Empty;

        public string Vendor { get; set; } = string.Empty;

        public string InvoiceNumber { get; set; } = string.Empty;

        public int CreatedBy { get; set; }
    }

    //public class AssetResponseDto
    //{
    //    public int Id { get; set; }
    //    public string AssetName { get; set; } = string.Empty;
    //    public string Status { get; set; } = string.Empty;
    //    public string CategoryName { get; set; } = string.Empty;
    //    public string SubCategoryName { get; set; } = string.Empty;
    //    public string SerialNumber { get; set; } = string.Empty;
    //}
}
