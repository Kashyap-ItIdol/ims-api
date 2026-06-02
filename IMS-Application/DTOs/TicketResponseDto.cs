namespace IMS_Application.DTOs
{
    public class TicketResponseDto
    {
        public TicketInfo ticket { get; set; } = new();
        public List<TicketCommentInfo> comments { get; set; } = new();
        public List<TicketAttachmentInfo> attachments { get; set; } = new();
        public List<TicketAssignmentInfo> assignments { get; set; } = new();
        public List<TicketStatusHistoryInfo> statusHistories { get; set; } = new();
    }

    public class TicketAssignmentInfo
    {
        public int Id { get; set; }
        public int assignedTo { get; set; }
        public string assignedToName { get; set; } = string.Empty;
        public int assignedBy { get; set; }
        public DateTime assignedAt { get; set; }
        public string status { get; set; } = string.Empty;
    }

    public class TicketStatusHistoryInfo
    {
        public int Id { get; set; }
        public int OldStatusId { get; set; }
        public string OldStatus { get; set; } = string.Empty;
        public int NewStatusId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public int ChangedBy { get; set; }
        public string ChangedByName { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
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
        public string? categoryName { get; set; }
        public string? subCategoryName { get; set; }

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

    public class TicketAttachmentInfo
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
