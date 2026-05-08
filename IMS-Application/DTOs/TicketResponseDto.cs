namespace IMS_Application.DTOs
{
    public class TicketResponseDto
    {
        public TicketInfo ticket { get; set; } = new();
        public List<TicketCommentInfo> comments { get; set; } = new();
    }

    public class TicketInfo
    {
        public string Id { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string TicketType { get; set; } = string.Empty;
        public string TicketPriority { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string createdAt { get; set; } = string.Empty;
        public string updatedAt { get; set; } = string.Empty;
        public string assetId { get; set; } = string.Empty;
        public string categoryId { get; set; } = string.Empty;
        public string subCategoryId { get; set; } = string.Empty;
        public UserInfo createdBy { get; set; } = new();
        public UserInfo? assignedTo { get; set; }
    }

    public class UserInfo
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
    }

    public class TicketCommentInfo
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string? UpdatedAt { get; set; }
        public int? ParentCommentId { get; set; }
        public int LikeCount { get; set; }
        public List<CommentReactionResponseDto> Reactions { get; set; } = new();
    }
}
