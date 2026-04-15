using MediatR;
using Microsoft.Extensions.Logging;
using SmartInventory.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SmartInventory.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
         : IPipelineBehavior<TRequest, TResponse>
         where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            if (request is ISensitiveRequest) 
            {
                logger.LogInformation("Handling sensitive request: {RequestName}", requestName);
            }
            else
            {
                logger.LogInformation("Handling request: {RequestName} with content: {@Request}", requestName, request);
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var response = await next();
                stopwatch.Stop();

                if (request is not ISensitiveRequest)
                    logger.LogInformation("Handled {RequestName} in {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex,
                    "Error handling {RequestName} after {ElapsedMs}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
