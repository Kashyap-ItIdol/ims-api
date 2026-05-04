using IMS_Application.Validators;
using System.ComponentModel.DataAnnotations;

namespace IMS_Application.DTOs
{
    public class CreateAssetDto
    {
        [StringLength(100, ErrorMessage = "Asset name cannot be more than 100 characters.")]
        public string AssetName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty; 

        [Range(1, int.MaxValue, ErrorMessage = "Category ID must be a greater than 1.")]
        public int CategoryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Subcategory ID must be a greater than 1.")]
        public int SubCategoryId { get; set; }

        [StringLength(50)]
        public string Brand { get; set; } = string.Empty;

        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [StringLength(50)]
        public string SerialNumber { get; set; } = string.Empty;

        public string Condition { get; set; } = string.Empty;

        [StringLength(100)]
        public string ClientPOC { get; set; } = string.Empty;

        [StringLength(100)]
        public string SalesPOC { get; set; } = string.Empty;
        [Range(1, int.MaxValue, ErrorMessage = "CreatedBy must be valid")]
        public int CreatedBy { get; set; }
    }

    public class AssetResponseDto
    {
        public int Id { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SubCategoryName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
    }
}
