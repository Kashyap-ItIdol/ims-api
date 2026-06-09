namespace IMS_Application.DTOs
{
    public class TicketCommentResponseDto
    {
        public int Id { get; set; }
        public string TicketId { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string? UpdatedAt { get; set; }
        public int? ParentCommentId { get; set; }
    }
}