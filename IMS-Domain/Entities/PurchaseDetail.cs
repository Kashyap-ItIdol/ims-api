using System;

namespace IMS_Domain.Entities
{
    public class PurchaseDetail : BaseEntity
    {
      //  public int Id { get; set; }

        public int InventoryId { get; set; }

        public required string Vendor { get; set; }

        public DateTime PurchaseDate { get; set; }
        public required DateTime WarrantyExpiry { get; set; }

        public decimal PurchaseCost { get; set; }
        public required string InvoiceNumber { get; set; }

        public required DateTime AmcExpiry { get; set; }

        // Navigation
        public required Inventory Inventory { get; set; }
    }
}