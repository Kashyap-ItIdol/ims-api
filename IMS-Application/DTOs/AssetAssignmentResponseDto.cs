namespace Application.DTOs;

public class AssetAssignmentResponseDto
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }   
    public Guid EmployeeId { get; set; }

    public DateTime AssignedDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }

    public string? OfficeNo { get; set; }
    public string? TableNo { get; set; }
}