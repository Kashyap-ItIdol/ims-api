namespace IMS_Application.DTOs
{
    public class GetCategoryDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public List<SubCategoryDto> SubCategories { get; set; } = new();
    }
}
