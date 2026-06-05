namespace IMS_Application.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public required string Title { get; set; }

        public required string Message { get; set; }

        public bool IsRead { get; set; }

        public string CreatedAt { get; set; } = string.Empty;
    }
}
