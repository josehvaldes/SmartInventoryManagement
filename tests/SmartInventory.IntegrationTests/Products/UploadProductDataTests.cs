using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Infrastructure.Data.Context;
using SmartInventory.Contracts.Requests.Products;
using SmartInventory.IntegrationTests.Common;
using SmartInventory.IntegrationTests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Net.Http;
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
            response.IsSuccessStatusCode.Should().BeTrue(await response.Content.ReadAsStringAsync());
            var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            document.Should().NotBeNull();
            var root = document!.RootElement;
            var fileUrl = root.GetProperty("imageUrl").GetString();
            fileUrl.Should().NotBeNullOrEmpty();
        }
    }
}
