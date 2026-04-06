using System.Collections.Generic;
using IMS_Domain.Constants;

namespace IMS_Domain.Entities
{
        public class Inventory : BaseEntity
        {
       // public int Id { get; set; }

        public required string ItemName { get; set; }

        public string? ImageUrl { get; set; }

        public required int CategoryId { get; set; }
        public required int SubCategoryId { get; set; }

        public required string Brand { get; set; }
        public required string Model { get; set; }
        public required string SerialNumber { get; set; }

        public required AssetStatus Status { get; set; }
        public required ConditionType Condition { get; set; }

       // public int PurchaseDetailId { get; set; }

        //public int AccesoryId { get; set; }

        //public int AssignmentId { get; set; }


        // Navigation
        public required Category Category { get; set; }
        public required SubCategory SubCategory { get; set; }

        public  PurchaseDetail? PurchaseDetail { get; set; }

        public ICollection<Accessory> Accessories { get; set; } = new List<Accessory>();
        public ICollection<InventoryAssignment> Assignments { get; set; } = new List<InventoryAssignment>();
    }
}