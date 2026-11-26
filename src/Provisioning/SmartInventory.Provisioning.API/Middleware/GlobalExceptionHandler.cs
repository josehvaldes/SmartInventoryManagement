using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
//using SmartInventory.Application.Common.Tenants;
using SmartInventory.Provisioning.Application.Common.Exceptions;

namespace SmartInventory.Provisioning.API.Middleware
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, title) = exception switch
            {
                //EntityNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                //ValidationException => (StatusCodes.Status422UnprocessableEntity, "Validation Error"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                //TenantException tenantException => (tenantException.StatusCode, "Tenant Error"),
                _ => (StatusCodes.Status500InternalServerError, "Server Error")
            };

            var detail = exception.Message;

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

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, problemDetails.GetType(), cancellationToken);

            return true; // true = exception is handled, do not propagate further
        }
    }
}
