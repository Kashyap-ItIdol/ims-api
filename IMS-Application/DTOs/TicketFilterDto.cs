namespace IMS_Application.DTOs
{
    public class TicketFilterDto
    {
        public List<string>? Status { get; set; }
        public List<string>? TicketPriority { get; set; }
        public List<int>? AssignTo { get; set; }
        public List<string>? TicketType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
