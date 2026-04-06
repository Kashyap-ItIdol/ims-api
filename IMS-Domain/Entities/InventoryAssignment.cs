using System;

namespace IMS_Domain.Entities
{
    public class InventoryAssignment : BaseEntity
    {
       // public int Id { get; set; }

        public int InventoryId { get; set; }

        public int? UserId { get; set; }  // for Assign to 

        public DateTime AssignedDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }

        public string Location { get; set; } = string.Empty;
        public string DeskNumber { get; set; } = string.Empty;

        public required Inventory Inventory { get; set; } 
        public User? User { get; set; } 
    }
}