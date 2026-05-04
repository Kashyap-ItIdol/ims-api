namespace IMS_Domain.Entities

{
    public class SubCategory
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public required string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        public bool IsActive { get; set; } = true;
        public ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
        public ICollection<Asset> Assets { get; set; }
    }
}