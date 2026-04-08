namespace IMS_Application.DTOs
{
    public class CreateTicketRequestDto
    {
        
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TicketType { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public int? AssetId { get; set; }

        public int assignedTo { get; set; }
    }
}
