namespace IMS_Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
        public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}
