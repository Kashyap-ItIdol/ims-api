namespace IMS_Application.DTOs
{
    public class TicketAttachmentResponseDto
    {
        public int AttachmentId { get; set; }
        public int TicketId { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
