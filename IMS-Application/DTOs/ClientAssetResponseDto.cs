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
        public string AssignedUserName { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
