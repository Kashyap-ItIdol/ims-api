using System.ComponentModel.DataAnnotations;

namespace IMS_Domain.Entities
{
    public class Asset
    {
        public int Id { get; set; }

        public string AssetName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public int SubCategoryId { get; set; }

        public string Brand { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public string SerialNumber { get; set; } = string.Empty;

        public string Condition { get; set; } = string.Empty;

        public string ClientPOC { get; set; } = string.Empty;

        public string SalesPOC { get; set; } = string.Empty;

        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
        public int? DeletedBy { get; set; }

        public SubCategory? SubCategory { get; set; }
        public Category? Category { get; set; }
    }
}
