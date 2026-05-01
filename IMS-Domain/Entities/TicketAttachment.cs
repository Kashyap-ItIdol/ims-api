namespace IMS_Domain.Entities
{
    public class TicketAttachment
    {

        public int Id { get; set; }
        public int TicketId { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }

        //navigation
        public User User { get; set; }
        public Ticket Ticket { get; set; }
    }
}
