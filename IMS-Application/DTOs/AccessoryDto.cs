using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.DTOs
{
    public class AccessoryDto
    {
        public string AccessoryName { get; set; } = null!;
        public DateTime PurchaseDate { get; set; }

        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }

        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;

        public int Condition { get; set; }
    }
}
