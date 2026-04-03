using System;
using IMS_Domain.Constants;

namespace IMS_Domain.Entities
{
    public class Accessory
    {
        public int Id { get; set; }

        public int InventoryId { get; set; }

        public required string AccessoryName { get; set; }

        public required DateTime PurchaseDate { get; set; }

        public int? CategoryId { get; set; }
        public int ?SubCategoryId { get; set; }

        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;

        public ConditionType Condition { get; set; }

        // Navigation
        public required Inventory Inventory { get; set; }
        public Category? Category { get; set; }
        public SubCategory? SubCategory { get; set; }
    }
}