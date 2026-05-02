namespace IMS_Application.DTOs
{
    public class TicketResponseDto
    {
        public TicketInfo ticket { get; set; } = new();
        public List<TicketCommentInfo> comments { get; set; } = new();
        public List<TicketAttachmentInfo> attachments { get; set; } = new();

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
        public UserInfo createdBy { get; set; } = new();
        public UserInfo? assignedTo { get; set; }
        public int? categoryId { get; set; }
        public string? categoryName { get; set; }
        public int? subCategoryId { get; set; }
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
    }

    public class TicketAttachmentInfo
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
