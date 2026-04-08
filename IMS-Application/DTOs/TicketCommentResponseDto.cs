namespace IMS_Application.DTOs
{
    public class TicketCommentResponseDto
    {
        public int Id { get; set; }
        public string ticketId { get; set; } = string.Empty;
       
        public string createdAt { get; set; } = string.Empty;
    }
}
