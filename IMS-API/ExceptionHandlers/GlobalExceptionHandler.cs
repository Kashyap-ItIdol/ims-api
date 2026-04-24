using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using IMS_Application.Common.Constants;

namespace IMS_API.ExceptionHandlers;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        if (exception is FluentValidation.ValidationException valEx)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            var errors = valEx.Errors.Select(e => new 
            { 
                Field = e.PropertyName,
                Message = e.ErrorMessage 
            }).ToList();

            var response = new
            {
                success = false,
                message = IMS_Application.Common.Constants.ErrorMessages.ValidationFailed,
                data = new { errors },
                statusCode = 400
            };

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }

        // Existing generic handling for other exceptions
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var responseMessage = _env.IsDevelopment()
            ? $"Server Error: {exception.GetType().Name}: {exception.Message}"
            : "An unexpected error occurred on the server.";

        var response500 = new
        {
            success = false,
            message = responseMessage,
            data = (object?)null,
            devDetails = _env.IsDevelopment() ? exception.ToString() : null
        };

        await httpContext.Response.WriteAsJsonAsync(response500, cancellationToken);

        return true;
    }
}

