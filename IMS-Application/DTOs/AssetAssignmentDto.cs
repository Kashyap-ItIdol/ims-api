namespace IMS_Application.DTOs
{
    public class AssetAssignmentDto
    {
        public int AssetId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public string? AssignedTo { get; set; }
        public int? UserId { get; set; }
        public string? Department { get; set; }
        public DateTime? AssignDate { get; set; }
        public string? OfficeNo { get; set; }
        public int? OfficeId { get; set; }
        public string? TableNo { get; set; }
        public int? TableId { get; set; }
        public NetworkDetailsDto? Network { get; set; }
        public List<AssetHistoryDto> History { get; set; } = new();
    }
}