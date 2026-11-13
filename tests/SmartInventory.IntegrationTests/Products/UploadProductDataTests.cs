using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Infrastructure.Data.Context;
using SmartInventory.Contracts.Requests.Products;
using SmartInventory.IntegrationTests.Infrastructure;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SmartInventory.IntegrationTests.Products
{
    [Collection("Integration")]
    public class UploadProductDataTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
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
        public async Task UploadProductData_ShouldReturnFileUrl()
        {
            // Arrange
            var product = ProductData.Products.First();
            var productSKU = product.SKU;
            var productName = product.Name;
            var fileContent = Encoding.UTF8.GetBytes("fake image content");
            var fileName = "testfile.jpg";
            var contentType = "image/jpeg";

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(productSKU), nameof(UploadProductRequest.ProductSKU));
            form.Add(new StringContent(productName), nameof(UploadProductRequest.ProductName));
            var fileContentPart = new ByteArrayContent(fileContent);
            fileContentPart.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            form.Add(fileContentPart, "file", fileName);

            // Act
            var response = await Client.PostAsync("/api/v1/products/upload", form, CancellationToken.None);
            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
            var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            document.Should().NotBeNull();
            var root = document!.RootElement;
            var fileUrl = root.GetProperty("imageUrl").GetString();
            fileUrl.Should().NotBeNullOrEmpty();

            var options = new DbContextOptionsBuilder<SmartInventoryDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;
            await using var context = new SmartInventoryDbContext(options);
            var updatedProduct = await context.Products.SingleAsync(product => product.SKU == productSKU, CancellationToken.None);
            updatedProduct.ImageUrl.Should().Be(fileUrl);
        }

        [Fact]
        public async Task UploadProductData_WithoutFile_ReturnsBadRequest()
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(ProductData.Products.First().SKU), nameof(UploadProductRequest.ProductSKU));
            form.Add(new StringContent(ProductData.Products.First().Name), nameof(UploadProductRequest.ProductName));

            var response = await Client.PostAsync("/api/v1/products/upload", form, CancellationToken.None);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            using var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            document!.RootElement.GetProperty("errors").TryGetProperty("file", out _).Should().BeTrue();
        }

        [Fact]
        public async Task UploadProductData_WithMissingProductDetails_ReturnsUnprocessableEntity()
        {
            using var form = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("fake image content"));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            form.Add(fileContent, "file", "testfile.jpg");

            var response = await Client.PostAsync("/api/v1/products/upload", form, CancellationToken.None);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
            using var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            var errors = document!.RootElement.GetProperty("errors");
            errors.TryGetProperty(nameof(UploadProductRequest.ProductSKU), out _).Should().BeTrue();
            errors.TryGetProperty(nameof(UploadProductRequest.ProductName), out _).Should().BeTrue();
        }

        [Fact]
        public async Task UploadProductData_WithUnsupportedFileExtension_ReturnsUnprocessableEntity()
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(ProductData.Products.First().SKU), nameof(UploadProductRequest.ProductSKU));
            form.Add(new StringContent(ProductData.Products.First().Name), nameof(UploadProductRequest.ProductName));
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("fake image content"));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            form.Add(fileContent, "file", "testfile.txt");

            var response = await Client.PostAsync("/api/v1/products/upload", form, CancellationToken.None);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
            using var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            document!.RootElement.GetProperty("errors")
                .TryGetProperty("FileName", out _)
                .Should().BeTrue();
        }

        [Fact]
        public async Task UploadProductData_WithUnknownProductSku_ReturnsNotFound()
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent("UNKNOWN-SKU"), nameof(UploadProductRequest.ProductSKU));
            form.Add(new StringContent("Unknown Product"), nameof(UploadProductRequest.ProductName));
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("fake image content"));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            form.Add(fileContent, "file", "testfile.jpg");

            var response = await Client.PostAsync("/api/v1/products/upload", form, CancellationToken.None);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UploadProductData_WithInvalidToken_ReturnsUnauthorized()
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(ProductData.Products.First().SKU), nameof(UploadProductRequest.ProductSKU));
            form.Add(new StringContent(ProductData.Products.First().Name), nameof(UploadProductRequest.ProductName));
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("fake image content"));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            form.Add(fileContent, "file", "testfile.jpg");
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/products/upload")
            {
                Content = form
            };
            request.Headers.Authorization = new("Bearer", TestAuthHandler.InvalidToken);

            var response = await Client.SendAsync(request, CancellationToken.None);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }
    }
}
