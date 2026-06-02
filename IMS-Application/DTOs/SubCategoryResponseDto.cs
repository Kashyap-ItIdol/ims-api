namespace IMS_Application.DTOs
{
    public class SubCategoryResponseDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public required string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public bool IsActive { get; set; }
    }
}