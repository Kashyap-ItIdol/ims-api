namespace IMS_Application.DTOs;

public class CreateAndAssignAssetDto
{
    public string ItemName { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public int SubCategoryId { get; set; }

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string? SerialNumber { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public string Vendor { get; set; } = string.Empty;

    public int ConditionId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string ClientPOC { get; set; } = string.Empty;

    public DateTime AssignedDate { get; set; }

    public string? OfficeNo { get; set; }

    public string? TableNo { get; set; }

    public DateTime? ExpectedReturnDate { get; set; }

    // EmployeeId will be set from context (not from form)
    public int EmployeeId { get; set; }
}
