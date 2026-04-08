using System.ComponentModel.DataAnnotations;

namespace IMS_Domain.Entities
{
    public class Ticket
    {
        
        public int Id { get; set; }

        public required string Title { get; set; }

        public string Description { get; set; } = string.Empty;

        public TicketType TicketType { get; set; }

        public TicketPriority TicketPriority { get; set; }

        public Status Status { get; set; } = Status.Open;

        public int CreatedBy { get; set; }

        public int? AssetId { get; set; } 

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
        public ICollection<TicketAssignment> TicketAssignments { get; set; } = new List<TicketAssignment>();
        public ICollection<TicketStatusHistory> TicketStatusHistories { get; set; } = new List<TicketStatusHistory>();
    }
}
