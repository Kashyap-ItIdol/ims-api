using Microsoft.AspNetCore.Http;

namespace IMS_Application.DTOs
{
    public class UploadAttachmentRequest
    {
        public IFormFile? File { get; set; }
    }
}
