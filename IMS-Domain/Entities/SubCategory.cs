namespace IMS_Domain.Entities
{
    public class SubCategory
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int CategoryId { get; set; } //foreign key
        public Category Category { get; set; } //navigation
        public ICollection<Inventory> Inventory { get; set; } //navigation
    }
}
