using System;
using System.Text.Json.Serialization;

namespace IMS_Application.DTOs
{
    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public bool Success { get; set; }
        public string Timestamp { get; set; } = string.Empty;

        public static ApiResponse<T> APIResponse(int statusCode, string message, T? data, bool success)
        {
            return new ApiResponse<T>
            {
                StatusCode = statusCode,
                Message = message,
                Data = data,
                Success = success,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }
    }
    
}

