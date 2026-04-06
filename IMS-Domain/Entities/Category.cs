using System.Collections.Generic;

namespace IMS_Domain.Entities
{
    public class Category : BaseEntity
    {
      //  public int Id { get; set; }

        public required string CategoryName { get; set; }

        // Navigation
        public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
        public ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
    }
}