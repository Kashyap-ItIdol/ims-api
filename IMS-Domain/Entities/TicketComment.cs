namespace IMS_Domain.Entities
{
    public class TicketComment
    {

        public int Id { get; set; }
        public int TicketId { get; set; }
        public int UserId { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
        public int? ParentCommentId { get; set; }
        public TicketComment? ParentComment { get; set; }
        public ICollection<TicketComment> Replies { get; set; } = new List<TicketComment>();
        public ICollection<TicketCommentLike> Likes { get; set; } = new List<TicketCommentLike>();
        public ICollection<TicketCommentReaction> Reactions { get; set; } = new List<TicketCommentReaction>();
    }
}
