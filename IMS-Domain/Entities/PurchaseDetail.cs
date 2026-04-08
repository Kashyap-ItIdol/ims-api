namespace IMS_Domain.Entities
{
    public class PurchaseDetail
    {
        public int Id { get; set; }

        public int InventoryId { get; set; }

        public Inventory Inventory { get; set; }

        public string Vendor { get; set; }

        public decimal PurchaseCost { get; set; }

        public string InvoiceNumber { get; set; }

        public DateTime PurchaseDate { get; set; }

        public DateTime? WarrantyExpiry { get; set; }

        public DateTime AmcExpiry { get; set; }
    }
}
