namespace IMS_Application.DTOs
{
    public class UpdateTicketStatusResponseDto
    {
        public string message { get; set; } = string.Empty;
        public string updatedStatus { get; set; } = string.Empty;
        public DateTime updatedAt { get; set; }
    }
}
