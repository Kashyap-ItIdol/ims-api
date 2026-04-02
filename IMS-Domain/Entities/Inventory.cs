namespace IMS_Domain.Entities
{
    public class Inventory
    {
        public int Id { get; set; }

        public string InventoryName { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; }

        public int SubcategoryId { get; set; }

        public SubCategory Subcategory { get; set; }


        public string Model { get; set; }

        public string SerialNumber { get; set; }

        public string Status { get; set; }

        public PurchaseDetail PurchaseDetail { get; set; }

        public ICollection<InventoryAssignment> InventoryAssignments { get; set; } = new List<InventoryAssignment>();

    }
}
