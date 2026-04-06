namespace IMS_Application.DTOs
{
    public class InventoryCreateDto
    {
        // Basic Info
        public string ItemName { get; set; } = null!;
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }

        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;

        public int Status { get; set; }
        public int Condition { get; set; }

        // Image
        public string? Image { get; set; } // 🔥 important

        // Purchase
        public PurchaseDetailDto PurchaseDetail { get; set; } = null!;

        // Accessories
        public List<AccessoryDto>? Accessories { get; set; }

        // Assignment
        public InventoryAssignmentDto? Assignment { get; set; }
    }
}
