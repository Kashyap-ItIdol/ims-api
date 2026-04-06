using Microsoft.AspNetCore.Diagnostics;

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
        // Log the full exception for the backend team
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // Maintain the strict JSON contract for the React frontend
        var response = new
        {
            success = false,
            message = _env.IsDevelopment()
                ? $"Server Error: {exception.Message}"
                : "An unexpected error occurred on the server.",
            data = (object?)null,
            // We can optionally pass the stack trace in a separate dev-only property
            devDetails = _env.IsDevelopment() ? exception.StackTrace : null
        };

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
