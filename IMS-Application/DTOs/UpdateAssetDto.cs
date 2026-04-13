namespace IMS_Application.DTOs
{
    public class UpdateAssetDto
    {
        public int Id { get; set; } 
        public MainAssetDto MainAsset { get; set; } = null!;
        public List<ChildActionDto> Children { get; set; } = new();

        
        public int? AssignedTo { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public string? Location { get; set; }
        public string? TableNo { get; set; }
    }

    public class MainAssetDto
    {
        public string ItemName { get; set; } = null!;
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string SerialNo { get; set; } = null!;
        public int StatusId { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int ConditionId { get; set; }

        public string? Vendor { get; set; }
        public decimal? PurchaseCost { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string? InvoiceNumber { get; set; }

        public DateTime? WarrantyExpiry { get; set; }
        public DateTime? AmcExpiry { get; set; }

        
    }

    public class ChildActionDto
    {
        public int? Id { get; set; } // null for new
        public string Action { get; set; } = null!; // add/update/delete/attach
        public AssetItemDto? Data { get; set; } // required for add/update
    }
}