namespace IMS_Application.Common.Models
{
    public class Result<T>
    {
        public bool IsSuccess { get; init; }
        public string? Message { get; init; }
        public int StatusCode { get; init; }
        public T? Data { get; init; }

        public static Result<T> Success(T data,string? message = null) =>
            new() { IsSuccess = true, Data = data, Message = message, StatusCode = 200 };

        public static Result<T> Failure(string message, int statusCode) =>
            new() { IsSuccess = false, Message = message, StatusCode = statusCode };
    }

}
