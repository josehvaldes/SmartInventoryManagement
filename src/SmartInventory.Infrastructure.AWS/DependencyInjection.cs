using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Infrastructure.AWS.Settings;
using SmartInventory.Infrastructure.AWS.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Infrastructure.AWS
{
    public static class DependencyInjection
    {

        public static IServiceCollection AddAWSDependencies(
                    this IServiceCollection services, IConfiguration config)
        {
            services.Configure<AwsSettings>(config.GetSection("AwsSettings"));
            services.AddPollyDependencies(config);
            services.AddScoped<IFileStorageService, S3FileStorageService>();
            
            return services;
        }

        public static IServiceCollection AddPollyDependencies(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IAsyncPolicy>(sp =>
            {
                var retry = PollyPolicies.GetRetryPolicy();
                var breaker = PollyPolicies.GetCircuitBreakerPolicy();

                return Policy.WrapAsync(retry, breaker);
            });

            return services;
        }
    }
}
