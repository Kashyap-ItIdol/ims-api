using System.ComponentModel.DataAnnotations;

namespace IMS_Application.DTOs
{
    public class CreateCategoryRequestDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
