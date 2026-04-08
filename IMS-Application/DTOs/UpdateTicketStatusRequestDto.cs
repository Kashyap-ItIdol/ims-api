

namespace IMS_Application.DTOs
{
    public class UpdateTicketStatusRequestDto
    {
        public int TicketId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
