using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.DTOs
{
    public class PurchaseDetailDto
    {
        public string Vendor { get; set; } = null!;
        public DateTime PurchaseDate { get; set; }
        public decimal PurchaseCost { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public DateTime WarrantyExpiry { get; set; }
        public DateTime AmcExpiry { get; set; }
    }
}
