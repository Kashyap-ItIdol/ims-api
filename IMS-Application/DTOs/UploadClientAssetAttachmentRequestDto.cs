using Microsoft.AspNetCore.Http;

namespace IMS_API.DTOs
{
    public class UploadClientAssetAttachmentRequestDto
    {
        public int AssetId { get; set; }
        public IFormFile File { get; set; }
    }
}
