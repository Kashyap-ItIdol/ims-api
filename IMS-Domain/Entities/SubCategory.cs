using System.Collections.Generic;

namespace IMS_Domain.Entities
{
    public class SubCategory : BaseEntity
    {
      //  public int Id { get; set; }

        public int CategoryId { get; set; }

        public  required string SubCategoryName { get; set; }

        

        // Navigation
        public required Category Category { get; set; }
        public ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
    }
}