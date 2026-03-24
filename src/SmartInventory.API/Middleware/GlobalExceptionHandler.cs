using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.Common.Exceptions;

namespace SmartInventory.API.Middleware
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, title) = exception switch
            {
                EntityNotFoundException    => (StatusCodes.Status404NotFound,            "Not Found"),
                ValidationException        => (StatusCodes.Status422UnprocessableEntity, "Validation Error"),
                UnauthorizedAccessException=> (StatusCodes.Status401Unauthorized,        "Unauthorized"),
                _                          => (StatusCodes.Status500InternalServerError, "Server Error")
            };

            logger.LogError(
                exception,
                "Exception: {Type} | Status: {StatusCode} | Message: {Message}",
                exception.GetType().Name,
                statusCode,
                exception.Message);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title  = title,
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true; // true = exception is handled, do not propagate further
        }
    }
}
