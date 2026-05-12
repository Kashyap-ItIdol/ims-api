namespace Application.DTOs;

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
}