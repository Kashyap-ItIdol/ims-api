namespace IMS_Application.DTOs
{
    public class CommentLikeResponseDto
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }
}

