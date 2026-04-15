namespace IMS_Domain.Entities
{
    public class TicketAssignment
    {

        public int Id { get; set; }

        public int TicketId { get; set; }

        public int assignedTo { get; set; }

        public int assigned_by { get; set; }

        public DateTime assigned_at { get; set; }

        public string status { get; set; } = "Active";
    }
}
