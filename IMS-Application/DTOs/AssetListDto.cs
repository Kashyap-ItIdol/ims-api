namespace IMS_Application.DTOs
{
    public class AssetListDto
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = null!;
        public string SerialNo { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string SubCategory { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? AssignedTo { get; set; }
    }
}
