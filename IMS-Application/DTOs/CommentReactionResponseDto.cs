namespace IMS_Application.DTOs
{
    public class CommentReactionResponseDto
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public string ReactionType { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }
}

