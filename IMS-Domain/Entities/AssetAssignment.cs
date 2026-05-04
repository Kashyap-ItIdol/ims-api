namespace IMS_Domain.Entities;

public class AssetAssignment
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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
}
