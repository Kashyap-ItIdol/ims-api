namespace IMS_Application.DTOs
{
    public class RecentActivityItemDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
    }
}



