namespace IMS_Application.DTOs
{
    public class ListCategoriesDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int CreatedBy { get; set; }
    }
}
