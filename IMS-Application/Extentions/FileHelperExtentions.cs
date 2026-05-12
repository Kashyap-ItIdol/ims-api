using System.IO;
using Microsoft.AspNetCore.Http;

namespace IMS_Application.Extentions
{
    public static class FileHelperExtentions
    {
        public static (byte[] fileData, string fileName, string contentType) GetFileData(this IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (null, string.Empty, string.Empty);

            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                var fileBytes = memoryStream.ToArray();
                var fileName = file.FileName;
                var contentType = file.ContentType;
                return (fileBytes, fileName, contentType);
            }
        }

        public static string GetFileExtension(this string fileName)
        {
            return Path.GetExtension(fileName).ToLowerInvariant();
        }

        public static bool IsImageFile(this string fileName)
        {
            var extension = fileName.GetFileExtension();
            return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif" || extension == ".bmp";
        }

        public static bool IsPdfFile(this string fileName)
        {
            var extension = fileName.GetFileExtension();
            return extension == ".pdf";
        }

        public static bool IsDocumentFile(this string fileName)
        {
            var extension = fileName.GetFileExtension();
            return extension == ".doc" || extension == ".docx" || extension == ".txt" || extension == ".pdf";
        }

        public static string GetContentType(this string fileName)
        {
            var extension = fileName.GetFileExtension();
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }
    }
}
