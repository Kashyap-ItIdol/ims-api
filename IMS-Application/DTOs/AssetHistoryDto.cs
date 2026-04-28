namespace IMS_Application.DTOs
{
    public class AssetHistoryDto
    {
        public int AssetId { get; set; }
        public string AssetName { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
