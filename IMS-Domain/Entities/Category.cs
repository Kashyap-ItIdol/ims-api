namespace IMS_Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<SubCategory> SubCategory { get; set; }
        public ICollection<Inventory> Inventory { get; set; }
    }
}
