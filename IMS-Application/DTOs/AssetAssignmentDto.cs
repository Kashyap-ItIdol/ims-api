namespace IMS_Application.DTOs
{
    public class AssetAssignmentDto
    {
        public string? AssignedTo { get; set; }
        public int? UserId { get; set; }
        public string? Department { get; set; }
        public DateTime? AssignDate { get; set; }
        public string? OfficeNo { get; set; }
        public string? TableNo { get; set; }

        public NetworkDetailsDto? Network { get; set; }

        public List<AssetHistoryDto> History { get; set; } = new();
    }
}
