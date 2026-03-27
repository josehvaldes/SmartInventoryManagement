using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

            services.AddSingleton<IAmazonS3>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<AwsSettings>>().Value;
                return new AmazonS3Client(
                    settings.AccessKey,
                    settings.SecretKey,
                    Amazon.RegionEndpoint.GetBySystemName(settings.Region));
            });

            services.AddPollyDependencies(config);
            services.AddSingleton<IFileStorageService, S3FileStorageService>();

            return services;
        }

        public static IServiceCollection AddPollyDependencies(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IAsyncPolicy>(sp =>
            {
                var retry = PollyPolicies.GetRetryPolicy();
                var breaker = PollyPolicies.GetCircuitBreakerPolicy();

                return Policy.WrapAsync(breaker, retry); // breaker outer: trips once after all retries fail
            });

            return services;
        }
    }
}
