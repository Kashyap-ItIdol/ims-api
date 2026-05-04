using System.ComponentModel.DataAnnotations;

namespace IMS_Application.DTOs;

public class AssetAssignmentDto
{
    [Required(ErrorMessage = "Asset ID is required")]
    public int AssetId { get; set; }

    [Required(ErrorMessage = "Employee ID is required")]
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "Assigned date is required")]
    public DateTime AssignedDate { get; set; }

    [Required(ErrorMessage = "Expected return date is required")]
    public DateTime? ExpectedReturnDate { get; set; }

    [StringLength(20, ErrorMessage = "Office number cannot exceed 20 characters")]
    public string? OfficeNo { get; set; }

    [StringLength(20, ErrorMessage = "Table number cannot exceed 20 characters")]
    public string? TableNo { get; set; }
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