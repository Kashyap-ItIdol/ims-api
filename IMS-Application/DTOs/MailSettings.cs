namespace IMS_Application.DTOs
{
    public class MailSettings
    {
        public string SmtpServer { get; set; } = null!;
        public int Port { get; set; }
        public string SenderEmail { get; set; } = null!;
        public string SenderPassword { get; set; } = null!;
        public string SenderName { get; set; } = "IMS";
    }
}
