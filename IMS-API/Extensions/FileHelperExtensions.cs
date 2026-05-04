using System.Text;

namespace IMS_API.Extensions
{
    public static class FileHelperExtensions
    {
        public static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            return extension switch
            {
                // Images
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                ".ico" => "image/x-icon",
                
                // Documents
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".html" or ".htm" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".csv" => "text/csv",
                ".rtf" => "application/rtf",
                
                // Microsoft Office
                ".doc" => "application/msword",
                ".dot" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".dotx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
                ".xls" => "application/vnd.ms-excel",
                ".xlt" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xltx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.template",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pot" => "application/vnd.ms-powerpoint",
                ".pps" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".potx" => "application/vnd.openxmlformats-officedocument.presentationml.template",
                ".ppsx" => "application/vnd.openxmlformats-officedocument.presentationml.slideshow",
                
                // Archives
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".7z" => "application/x-7z-compressed",
                ".tar" => "application/x-tar",
                ".gz" => "application/gzip",
                
                // Media
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".mp4" => "video/mp4",
                ".avi" => "video/x-msvideo",
                ".mov" => "video/quicktime",
                ".wmv" => "video/x-ms-wmv",
                ".flv" => "video/x-flv",
                ".webm" => "video/webm",
                
                // Other
                ".exe" => "application/octet-stream",
                ".dll" => "application/octet-stream",
                ".bat" => "application/octet-stream",
                ".cmd" => "application/octet-stream",
                
                _ => "application/octet-stream"
            };
        }

        public static bool IsImage(string fileName)
        {
            var contentType = GetContentType(fileName);
            return contentType.StartsWith("image/");
        }

        public static bool IsPdf(string fileName)
        {
            return GetContentType(fileName) == "application/pdf";
        }

        public static bool IsText(string fileName)
        {
            var contentType = GetContentType(fileName);
            return contentType.StartsWith("text/");
        }

        public static bool IsVideo(string fileName)
        {
            var contentType = GetContentType(fileName);
            return contentType.StartsWith("video/");
        }

        public static bool IsAudio(string fileName)
        {
            var contentType = GetContentType(fileName);
            return contentType.StartsWith("audio/");
        }
    }
}
