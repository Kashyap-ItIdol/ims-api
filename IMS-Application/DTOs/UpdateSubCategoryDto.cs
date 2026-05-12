namespace IMS_Application.DTOs
{
    public class UpdateSubCategoryDto
    {
        public int CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
