using System.ComponentModel.DataAnnotations;

namespace IMS_Application.DTOs;

public class CreateAndAssignAssetDto
{
    [Required(ErrorMessage = "Item name is required")]
    [StringLength(100, ErrorMessage = "Item name cannot exceed 100 characters")]
    public string ItemName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Category ID must be greater than 0")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Subcategory is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Subcategory ID must be greater than 0")]
    public int SubCategoryId { get; set; }

    [Required(ErrorMessage = "Brand is required")]
    [StringLength(50, ErrorMessage = "Brand cannot exceed 50 characters")]
    public string Brand { get; set; } = string.Empty;

    [Required(ErrorMessage = "Model is required")]
    [StringLength(50, ErrorMessage = "Model cannot exceed 50 characters")]
    public string Model { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Serial number cannot exceed 50 characters")]
    public string? SerialNumber { get; set; }

    [Required(ErrorMessage = "Condition is required")]
    [StringLength(50, ErrorMessage = "Condition cannot exceed 50 characters")]
    public string Condition { get; set; } = string.Empty;

    [Required(ErrorMessage = "Client POC is required")]
    [StringLength(100, ErrorMessage = "Client POC cannot exceed 100 characters")]
    public string ClientPOC { get; set; } = string.Empty;

    [Required(ErrorMessage = "Assigned date is required")]
    public DateTime AssignedDate { get; set; }

    [StringLength(20, ErrorMessage = "Office number cannot exceed 20 characters")]
    public string? OfficeNo { get; set; }

    [StringLength(20, ErrorMessage = "Table number cannot exceed 20 characters")]
    public string? TableNo { get; set; }

    [Required(ErrorMessage = "Expected return date is required")]
    public DateTime? ExpectedReturnDate { get; set; }

    // EmployeeId will be set from context (not from form)
    public int EmployeeId { get; set; }
}
