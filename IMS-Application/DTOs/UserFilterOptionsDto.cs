namespace IMS_Application.DTOs
{
    public class UserFilterOptionsDto
    {
        public ListOption FullNames { get; set; } = new();
        public ListOption Roles { get; set; } = new();
        public ListOption Departments { get; set; } = new();
        public ListOption Statuses { get; set; } = new();

        public class ListOption
        {
            public List<CheckboxOption> Options { get; set; } = new();
        }

        public class CheckboxOption
        {
            public string Key { get; set; } = string.Empty;
            public string Label { get; set; } = string.Empty;
        }
    }
}

