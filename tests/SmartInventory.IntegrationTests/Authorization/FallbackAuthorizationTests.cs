using FluentAssertions;
using Microsoft.Data.SqlClient;
using SmartInventory.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SmartInventory.IntegrationTests.Authorization
{
    [Collection("Integration")]
    public class FallbackAuthorizationTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
    {
        protected override Task SeedDataAsync(SqlConnection connection) => Task.CompletedTask;

        [Theory]
        [InlineData("/health/ready")]
        [InlineData("/health/live")]
        [InlineData("/metrics")]
        [InlineData("/scalar/v1")]
        public async Task EndpointWithoutAuthorize_WithInvalidToken_ReturnsUnauthorized(string path)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Authorization = new("Bearer", TestAuthHandler.InvalidToken);

            using var response = await Client.SendAsync(request, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task EndpointWithoutAuthorize_WithValidMembership_ReturnsSuccess()
        {
            using var response = await Client.GetAsync("/health/live", CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task EndpointWithoutAuthorize_WithoutMembership_ReturnsForbidden()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
            request.Headers.Authorization = new("Bearer", TestAuthHandler.NoMembershipToken);

            using var response = await Client.SendAsync(request, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AnonymousSession_WithInvalidToken_ReachesValidation()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/session");
            request.Headers.Authorization = new("Bearer", TestAuthHandler.InvalidToken);
            request.Content = JsonContent.Create(new { Username = "", Password = "" });

            using var response = await Client.SendAsync(request, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
    }
}
