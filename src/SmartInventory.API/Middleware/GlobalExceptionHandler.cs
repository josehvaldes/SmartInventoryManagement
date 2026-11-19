using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;
using SmartInventory.Application.Common.Exceptions;
using SmartInventory.Application.Common.Tenants;
using SmartInventory.Observability.AppMetrics;

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
                ValidationException => (StatusCodes.Status422UnprocessableEntity, "Validation Error"),
                UnauthorizedAccessException=> (StatusCodes.Status401Unauthorized,        "Unauthorized"),
                BrokenCircuitException=> (StatusCodes.Status503ServiceUnavailable, "Service temporarily Unavailable"),
                TenantException tenantException => (tenantException.StatusCode, "Tenant Error"),
                _                          => (StatusCodes.Status500InternalServerError, "Server Error")
            };

            switch (statusCode)
            {
                case StatusCodes.Status401Unauthorized:
                    WebMetrics.AuthenticationErrors.Inc();
                    break;
                case StatusCodes.Status503ServiceUnavailable:
                    WebMetrics.ServiceUnavailableErrors.Inc();
                    break;
                default:
                    break;
            }
            // Tenant errors expose only a code and a client-safe message; the internal detail stays in the logs.
            var tenantError = exception as TenantException;
            var detail = tenantError is not null
                ? TenantErrorCodes.GetPublicMessage(tenantError.ErrorCode)
                : exception.Message;

            if (statusCode >= 500)
            {
                logger.LogError(
                exception,
                "Exception: {Type} | Status: {StatusCode} | Message: {Message}",
                exception.GetType().Name,
                statusCode,
                exception.Message);
            }
            else 
            {
                logger.LogWarning(
                exception,
                "Exception: {Type} | Status: {StatusCode} | ErrorCode: {ErrorCode} | Message: {Message}",
                exception.GetType().Name,
                statusCode,
                tenantError?.ErrorCode,
                exception.Message);
            }            

            ProblemDetails problemDetails = exception switch
            {
                ValidationException validationException => new ValidationProblemDetails(validationException.Errors)
                {
                    Status = statusCode,
                    Title = title,
                    Detail = detail,
                    Instance = httpContext.Request.Path
                },
                _ => new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = detail,
                    Instance = httpContext.Request.Path
                }
            };

            if (tenantError is not null)
            {
                problemDetails.Extensions["errorCode"] = tenantError.ErrorCode;
            }

            httpContext.Response.StatusCode = statusCode;
            
            await httpContext.Response.WriteAsJsonAsync(problemDetails, problemDetails.GetType(), cancellationToken);

            return true; // true = exception is handled, do not propagate further
        }
    }
}
