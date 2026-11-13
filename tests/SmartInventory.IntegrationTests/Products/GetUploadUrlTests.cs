using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Contracts.Requests.Products;
using SmartInventory.Infrastructure.Data.Context;
using SmartInventory.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;


namespace SmartInventory.IntegrationTests.Products
{
    [Collection("Integration")]
    public class GetUploadUrlTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
    {
        protected override async Task SeedDataAsync(SqlConnection connection)
        {
            var options = new DbContextOptionsBuilder<SmartInventoryDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;

            await using var context = new SmartInventoryDbContext(options);
            await context.Products.AddAsync(ProductData.Products.First());
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetUploadUrl_ShouldReturnUploadUrl()
        {

            var product = ProductData.Products.First();
            var productSKU = product.SKU;
            var productName = product.Name;
            var getUploadUrlRequest = new GetUploadUrlRequest()
            {
                ProductSku = productSKU,
                FileName = "testfile.jpg",
                ContentType = "image/jpeg"
            };

            var response = await Client.PostAsJsonAsync($"/api/v1/products/upload-url", getUploadUrlRequest, CancellationToken.None);

            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            document.Should().NotBeNull();
            var root = document!.RootElement;
            var presignedUrl = root.GetProperty("uploadUrl").GetString();
            presignedUrl.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetUploadUrl_WithMissingRequiredFields_ReturnsUnprocessableEntity()
        {
            var response = await Client.PostAsJsonAsync("/api/v1/products/upload-url", new GetUploadUrlRequest(), CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            using var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            var errors = document!.RootElement.GetProperty("errors");
            errors.TryGetProperty(nameof(GetUploadUrlRequest.ProductSku), out _).Should().BeTrue();
            errors.TryGetProperty(nameof(GetUploadUrlRequest.FileName), out _).Should().BeTrue();
            errors.TryGetProperty(nameof(GetUploadUrlRequest.ContentType), out _).Should().BeTrue();
        }

        [Fact]
        public async Task GetUploadUrl_WithUnsupportedFileExtension_ReturnsUnprocessableEntity()
        {
            var request = new GetUploadUrlRequest
            {
                ProductSku = ProductData.Products.First().SKU,
                FileName = "testfile.txt",
                ContentType = "text/plain"
            };

            var response = await Client.PostAsJsonAsync("/api/v1/products/upload-url", request, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            using var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            document!.RootElement.GetProperty("errors")
                .TryGetProperty(nameof(GetUploadUrlRequest.FileName), out _)
                .Should().BeTrue();
        }

        [Fact]
        public async Task GetUploadUrl_WithInvalidToken_ReturnsUnauthorized()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/products/upload-url")
            {
                Content = JsonContent.Create(new GetUploadUrlRequest
                {
                    ProductSku = ProductData.Products.First().SKU,
                    FileName = "testfile.jpg",
                    ContentType = "image/jpeg"
                })
            };
            request.Headers.Authorization = new("Bearer", TestAuthHandler.InvalidToken);

            var response = await Client.SendAsync(request, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
