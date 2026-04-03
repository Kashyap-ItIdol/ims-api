namespace IMS_Application.DTOs
{ 

public class InventoryDto
{
    public string AssetName { get; set; }

    public int CategoryId { get; set; }
    public int SubCategoryId { get; set; }

    public string Brand { get; set; }
    public string Model { get; set; }
    public string SerialNumber { get; set; }

    public string Status { get; set; }
    public string Condition { get; set; }

    public PurchaseCreateDto Purchase { get; set; }
    public List<AccessoryCreateDto> Accessories { get; set; }
    public AssignmentCreateDto Assignment { get; set; }
}

public class PurchaseCreateDto
{
    public string Vendor { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public string InvoiceNumber { get; set; }
    public DateTime? WarrantyExpiry { get; set; }
    public DateTime? AmcExpiry { get; set; }
}

public class AccessoryCreateDto
{
    public string Name { get; set; }
    public int CategoryId { get; set; }
    public int SubCategoryId { get; set; }

    public string Brand { get; set; }
    public string Model { get; set; }
    public string SerialNumber { get; set; }

    public string Condition { get; set; }
}

public class AssignmentCreateDto
{
    public int? AssignedTo { get; set; }

    public DateTime AssignedDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }

    public string Location { get; set; }
    public string DeskNumber { get; set; }
}
    }