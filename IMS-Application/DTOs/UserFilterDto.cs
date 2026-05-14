namespace IMS_Application.DTOs
{
    public class UserFilterDto
    {
        public List<string>? FullNames { get; set; }
        public List<string>? RoleKeys { get; set; }
        public List<string>? DepartmentNames { get; set; }
        public List<string>? StatusKeys { get; set; }
    }
}

