namespace IMS_Domain.Entities
{
    public class Asset
    {
        public int Id { get; set; }
        public string? Image { get; set; }
        public required string ItemName { get; set; }
        public required int StatusId { get; set; }
        public required int CategoryId { get; set; }
        public required int SubCategoryId { get; set; }
        public required int ConditionId { get; set; }
        public required string Brand { get; set; }
        public required string Model { get; set; }
        public required string SerialNo { get; set; }
        public int? ParentAssetId { get; set; }
        public Asset? ParentAsset { get; set; }
        public ICollection<Asset> ChildAssets { get; set; } = new List<Asset>();
        public required string Vendor { get; set; }
        public required decimal PurchaseCost { get; set; }
        public required DateTime PurchaseDate { get; set; }
        public required string InvoiceNumber { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public DateTime? AmcExpiry { get; set; }    
        public int? AssignedTo { get; set; }    // user 
        public User? AssignedUser { get; set; }
        public DateTime? AssignDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public bool IsClient { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public Category Category { get; set; } = null!;
        public SubCategory SubCategory { get; set; } = null!;
        public AssetStatus AssetStatus { get; set; } = null!;
        public AssetCondition AssetCondition { get; set; } = null!;
    }
}