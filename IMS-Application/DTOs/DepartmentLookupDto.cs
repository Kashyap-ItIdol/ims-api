namespace IMS_Application.DTOs
{
    public class DepartmentLookupDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<UserInfoDto> Users { get; set; } = new();
    }
}
