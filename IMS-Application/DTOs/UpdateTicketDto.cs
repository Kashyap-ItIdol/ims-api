namespace IMS_Application.DTOs
{
    public class UpdateTicketDto
    {

        public string TicketTitle { get; set; }
        public List<string>? TicketType { get; set; }
        public List<string>? TicketPriority { get; set; }
        public int AssignedTo { get; set; }
        public int? AssetId { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public string Description { get; set; }
    }
}
