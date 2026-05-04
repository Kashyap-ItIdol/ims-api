using System.ComponentModel.DataAnnotations;

namespace IMS_Application.DTOs
{
    public class CreateClientAssetDto
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Asset name must be between 2 and 100 characters")]
        public required string AssetName { get; set; }

        public required string Status { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Asset ID must be greater than 0")]
        public int AssetId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Category ID must be greater than 0")]
        public int CategoryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "SubCategory ID must be greater than 0")]
        public int SubCategoryId { get; set; }

        public required string Brand { get; set; }

        public required string Model { get; set; }

        public required string SerialNumber { get; set; }

        public required string Condition { get; set; }

        public required string ClientName { get; set; }

        public required string ClientPOC { get; set; }

        public required string SalesPOC { get; set; }

        public required string Location { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Desk number must be greater than 0")]
        public int DeskNumber { get; set; }

        public int? AssignedTo { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Purchase cost cannot be negative")]
        public decimal PurchaseCost { get; set; }

        public int? CreatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}