using Amazon.S3;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly;

namespace SmartInventory.Infrastructure.AWS
{
    public static class PollyPolicies
    {
        public static AsyncRetryPolicy GetRetryPolicy()
        {
            return Policy
                .Handle<AmazonS3Exception>(ex =>
                    ex.StatusCode == System.Net.HttpStatusCode.InternalServerError ||
                    ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                    ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .Or<System.Net.Http.HttpRequestException>()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                  + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)) // jitter
                );
        }

        public static AsyncCircuitBreakerPolicy GetCircuitBreakerPolicy()
        {
            return Policy
                .Handle<AmazonS3Exception>()
                .Or<System.Net.Http.HttpRequestException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );
        }
    }
}
