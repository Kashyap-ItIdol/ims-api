namespace IMS_Domain.DTOs
{
    public class AddTicketCommentRequestDto
    {
        public int TicketId { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
