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
        _logger.LogError(exception, "Unhandled exception");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        if (_env.IsDevelopment())
        {
            await httpContext.Response.WriteAsJsonAsync(
                new { message = "An unexpected error occurred.", detail = exception.Message },
                cancellationToken);
        }
        else
        {
            await httpContext.Response.WriteAsJsonAsync(
                new { message = "An unexpected error occurred." },
                cancellationToken);
        }

        return true;
    }
}
