namespace IMS_Domain.Entities
{
    public class InventoryAssignment
    {
        public int Id { get; set; }

        public int InventoryId { get; set; }

        public Inventory Inventory { get; set; }

        public int AssignedTo { get; set; }

        public DateTime AssignedDate { get; set; }

        public DateTime? ExpectedReturnDate { get; set; }

        public string Location { get; set; }

        public string Table { get; set; }
    }
}
