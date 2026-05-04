namespace IMS_Application.DTOs
{
    public class UploadAttachmentDto
    {
        public int ClientAssetId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
    }

    public class AttachmentResponseDto
    {
        public int Id { get; set; }
        public int ClientAssetId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public bool IsDeleted { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}