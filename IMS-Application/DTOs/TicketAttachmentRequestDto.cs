using Microsoft.AspNetCore.Http;
namespace IMS_Application.DTOs
{
    public class TicketAttachmentRequestDto
    {
        public List<IFormFile> Files { get; set; } = new();
    }
}
