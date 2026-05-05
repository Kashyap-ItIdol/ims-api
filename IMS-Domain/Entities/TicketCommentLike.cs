namespace IMS_Domain.Entities
{
    public class TicketCommentLike
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
        public TicketComment Comment { get; set; } = null!;
    }
}

