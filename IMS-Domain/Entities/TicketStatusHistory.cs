namespace IMS_Domain.Entities
{
    public class TicketStatusHistory
    {

        public int Id { get; set; }

        public int TicketId { get; set; }

        public int OldStatusId { get; set; }
        public int NewStatusId { get; set; }

        public int ChangedBy { get; set; }

        public DateTime ChangedAt { get; set; }
    }
}
