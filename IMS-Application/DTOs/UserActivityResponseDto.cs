namespace IMS_Application.DTOs
{
    public class UserActivityResponseDto
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string RelatedId { get; set; } = string.Empty;

        public string User { get; set; } = string.Empty;

        public DateTime DateTime { get; set; }
    }
}