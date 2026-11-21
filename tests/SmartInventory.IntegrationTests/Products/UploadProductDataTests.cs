using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Contracts.Requests.Products;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Identity;
using SmartInventory.Infrastructure.Data.Context;
using SmartInventory.IntegrationTests.Data;
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
        private readonly List<Tenant> _tenants = TenantData.LoadTenantsFromFile();
        private readonly List<Product> _products = ProductData.LoadProductsFromFile();

        protected override async Task SeedDataAsync(SqlConnection connection)
        {
            await using var context = CreateSmartInventoryDbContext(false);
            await using var authContext = CreateAuthDbContext();

            foreach (var tenant in _tenants)
            {
                if (!await authContext.Tenants.AnyAsync(t => t.Id == tenant.Id))
                {
                    await authContext.Tenants.AddAsync(tenant);
                }
            }

            await authContext.SaveChangesAsync();
            foreach (var product in _products)
            {
                if (!await context.Products.AnyAsync(p => p.SKU == product.SKU))
                {
                    product.TenantId = SystemTenants.LegacyTenantId;
                    await context.Products.AddAsync(product);
                }
                    
            }

            await context.SaveChangesAsync();

        }

        [Fact]
        public async Task UploadProductData_ShouldReturnFileUrl()
        {
            // Arrange
            var product = _products.First();
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
            await using var context = CreateSmartInventoryDbContext();
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
            form.Add(new StringContent(_products.First().SKU), nameof(UploadProductRequest.ProductSKU));
            form.Add(new StringContent(_products.First().Name), nameof(UploadProductRequest.ProductName));
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
            form.Add(new StringContent(_products.First().SKU), nameof(UploadProductRequest.ProductSKU));
            form.Add(new StringContent(_products.First().Name), nameof(UploadProductRequest.ProductName));
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
