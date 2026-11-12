using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Domain.Identity;
using SmartInventory.Infrastructure.Data.Context;
using SmartInventory.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace SmartInventory.IntegrationTests.Sessions
{
    [Collection("Integration")]
    public class SessionTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
    {
        private User defaultUser = null!;

        protected override async Task SeedDataAsync(SqlConnection connection)
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;
            await using var context = new AuthDbContext(options);
            var users = UserData.LoadUsersFromFile();

            await context.Users.AddRangeAsync(users);
            defaultUser = users.First(user => user.Username == "admin");

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task Session_WithValidCredentials_ReturnsAccessTokenAndSecureRefreshCookie()
        {
            using var response = await LoginAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            using var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            document.Should().NotBeNull();

            var body = document!.RootElement;
            body.GetProperty("accessToken").GetString().Should().NotBeNullOrWhiteSpace();
            body.GetProperty("tokenType").GetString().Should().Be("Bearer");
            body.GetProperty("expiresIn").GetInt32().Should().BePositive();
            body.GetProperty("username").GetString().Should().Be(defaultUser.Username);
            body.TryGetProperty("rawRefreshToken", out _).Should().BeFalse();

            var cookie = GetSetCookie(response).ToLowerInvariant();
            cookie.Should().Contain("httponly");
            cookie.Should().Contain("secure");
            cookie.Should().Contain("samesite=strict");
            GetRefreshToken(response).Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Session_WithMissingCredentials_ReturnsUnprocessableEntity()
        {
            using var response = await Client.PostAsJsonAsync(
                "/api/v1/auth/session",
                new { Username = "", Password = "" },
                CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task Session_WithInvalidPassword_ReturnsUnauthorized()
        {
            using var response = await Client.PostAsJsonAsync(
                "/api/v1/auth/session",
                new { Username = defaultUser.Username, Password = "incorrect-password" },
                CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RefreshToken_WithoutCookie_ReturnsUnauthorized()
        {
            using var response = await Client.PostAsync("/api/v1/auth/refresh-token", content: null);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RefreshToken_WithValidCookie_RotatesRefreshToken()
        {
            using var loginResponse = await LoginAsync();
            var originalRefreshToken = GetRefreshToken(loginResponse);

            using var refreshResponse = await PostWithRefreshCookieAsync("/api/v1/auth/refresh-token", originalRefreshToken);

            refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            using var document = await refreshResponse.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            document.Should().NotBeNull();
            document!.RootElement.GetProperty("accessToken").GetString().Should().NotBeNullOrWhiteSpace();
            document.RootElement.TryGetProperty("rawRefreshToken", out _).Should().BeFalse();

            var rotatedRefreshToken = GetRefreshToken(refreshResponse);
            rotatedRefreshToken.Should().NotBe(originalRefreshToken);

            using var replayResponse = await PostWithRefreshCookieAsync("/api/v1/auth/refresh-token", originalRefreshToken);
            replayResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Revoke_WithRefreshCookie_ClearsCookieAndRevokesToken()
        {
            using var loginResponse = await LoginAsync();
            var refreshToken = GetRefreshToken(loginResponse);

            using var revokeResponse = await PostWithRefreshCookieAsync("/api/v1/auth/revoke", refreshToken);

            revokeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var deletionCookie = GetSetCookie(revokeResponse).ToLowerInvariant();
            deletionCookie.Should().Contain("refreshtoken=");
            deletionCookie.Should().Contain("expires=");

            using var refreshResponse = await PostWithRefreshCookieAsync("/api/v1/auth/refresh-token", refreshToken);
            refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Revoke_WithoutValidAuthorization_ReturnsUnauthorized()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/revoke");
            request.Headers.Authorization = new("Bearer", TestAuthHandler.InvalidToken);

            using var response = await Client.SendAsync(request, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        private Task<HttpResponseMessage> LoginAsync() => Client.PostAsJsonAsync(
            "/api/v1/auth/session",
            new { Username = defaultUser.Username, Password = "Admin@123!" },
            CancellationToken.None);

        private async Task<HttpResponseMessage> PostWithRefreshCookieAsync(string path, string refreshToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, path);
            request.Headers.Add("Cookie", $"refreshToken={refreshToken}");
            return await Client.SendAsync(request, CancellationToken.None);
        }

        private static string GetRefreshToken(HttpResponseMessage response)
        {
            var cookie = GetSetCookie(response).Split(';', 2)[0];
            return cookie["refreshToken=".Length..];
        }

        private static string GetSetCookie(HttpResponseMessage response) => response.Headers
            .GetValues("Set-Cookie")
            .Single(cookie => cookie.StartsWith("refreshToken=", StringComparison.OrdinalIgnoreCase));
    }
}
