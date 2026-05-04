using System.ComponentModel.DataAnnotations;

namespace IMS_Domain.Entities
{
    public class ClientAsset
    {
        public int Id { get; set; }

        public required string AssetName { get; set; }

        public required string Status { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int SubCategoryId { get; set; }
        public SubCategory? SubCategory { get; set; }

        public required string Brand { get; set; }

        public required string Model { get; set; }

        public required string SerialNumber { get; set; }

        public required string Condition { get; set; }

        public string? ItemPhoto { get; set; }

        public required string ClientName { get; set; }

        public required string ClientPOC { get; set; }

        public required string SalesPOC { get; set; }

        public string Location { get; set; } = string.Empty;

        public int DeskNumber { get; set; }

        public int? AssignedTo { get; set; }          

        public DateTime? AssignedDate { get; set; }  

        public User? AssignedUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DeletedAt { get; set; }

        public int? DeletedBy { get; set; }

        public bool IsDeleted { get; set; } = false;

        public ICollection<ClientAssetAttachment> Attachments { get; set; } = new List<ClientAssetAttachment>();
    }
}