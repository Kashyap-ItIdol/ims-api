namespace IMS_Application.DTOs;


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

        public string? TableNo { get; set; }
        public NetworkDetailsDto? Network { get; set; }
        public List<AssetHistoryDto> History { get; set; } = new();
    }

public class AssetAssignmentResponseDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public int EmployeeId { get; set; }

    public DateTime AssignedDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }

    public string? OfficeNo { get; set; }
    public string? TableNo { get; set; }
    public bool IsReturned => ActualReturnDate.HasValue;

    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
}
