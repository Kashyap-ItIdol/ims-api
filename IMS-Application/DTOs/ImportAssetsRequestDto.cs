using Microsoft.AspNetCore.Http;

namespace IMS_Application.DTOs
{
    public class ImportAssetsRequestDto
    {
        public IFormFile? CsvFile { get; set; }
    }
}

