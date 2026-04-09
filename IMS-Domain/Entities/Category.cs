using System.Collections.Generic;

namespace IMS_Domain.Entities

{

    public class Category
    {

        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();

        public ICollection<Asset> Assets { get; set; } = new List<Asset>();

    }


}