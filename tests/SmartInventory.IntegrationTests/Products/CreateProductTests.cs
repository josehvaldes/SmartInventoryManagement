using FluentAssertions;
using Microsoft.Data.SqlClient;
using SmartInventory.Contracts.Requests.Products;
using SmartInventory.Contracts.Responses.Products;
using SmartInventory.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace SmartInventory.IntegrationTests.Products
{
    [Collection("Integration")]
    public class CreateProductTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
    {
        protected override Task SeedDataAsync(SqlConnection connection)
        {
            // No initial data to seed for this test class
            return Task.CompletedTask;
        }

        private async Task<ProductResponse> GetProduct(Guid id)
        {
            var response = await Client.GetAsync($"/api/v1/products/{id}", CancellationToken.None);
            response.EnsureSuccessStatusCode();
            var product = await response.Content.ReadFromJsonAsync<ProductResponse>(cancellationToken: CancellationToken.None);
            product.Should().NotBeNull();
            return product!;
        }

        [Fact]
        public async Task CreateProduct_ReturnsExpectedLinks() 
        {
            var newProduct = ProductData.Product1;
            var response = await Client.PostAsJsonAsync("/api/v1/products", newProduct, CancellationToken.None);
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

            using var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            var root = document!.RootElement;
            var id = root.GetProperty("id").GetGuid();
            id.Should().NotBeEmpty();

            var links = root.GetProperty("links")
                .EnumerateArray()
                .ToArray();

            links.Select(link => link.GetProperty("rel").GetString())
                .Should()
                .BeEquivalentTo("self", "delete-product", "create-product");
        }

        [Fact]
        public async Task CreateProduct_ValidateSelfLink()
        {
            var newProduct = ProductData.Product1;
            var response = await Client.PostAsJsonAsync("/api/v1/products", newProduct, CancellationToken.None);
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            using var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            var root = document!.RootElement;
            var id = root.GetProperty("id").GetGuid();
            id.Should().NotBeEmpty();
            var links = root.GetProperty("links")
                .EnumerateArray()
                .ToArray();
            var selfLink = links.FirstOrDefault(link => link.GetProperty("rel").GetString() == "self");
            selfLink.Should().NotBeNull();
            var href = selfLink.GetProperty("href").GetString();
            href.Should().NotBeNullOrEmpty();
            Uri.TryCreate(href, UriKind.Absolute, out var uri).Should().BeTrue($"The href '{href}' should be a valid absolute URL.");

            var getResponse = await Client.GetAsync(uri!, CancellationToken.None);
            getResponse.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task CreateProduct_PersistsProductAndReturnsItsDetails()
        {
            var response = await Client.PostAsJsonAsync("/api/v1/products", ProductData.Product1, CancellationToken.None);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            using var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            var id = document!.RootElement.GetProperty("id").GetGuid();
            var product = await GetProduct(id);

            product.Id.Should().Be(id);
            product.SKU.Should().Be(ProductData.Product1.SKU);
            product.Name.Should().Be(ProductData.Product1.Name);
            product.Description.Should().Be(ProductData.Product1.Description);
            product.Category.Should().Be(ProductData.Product1.Category);
            product.UnitOfMeasure.Should().Be(ProductData.Product1.UnitOfMeasure);
            product.UnitCost.Should().Be(ProductData.Product1.UnitCost);
            product.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task CreateProduct_WithMissingRequiredFields_ReturnsUnprocessableEntity()
        {
            var response = await Client.PostAsJsonAsync("/api/v1/products", new CreateProductRequest(), CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            using var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            var errors = document!.RootElement.GetProperty("errors");

            errors.TryGetProperty("SKU", out _).Should().BeTrue();
            errors.TryGetProperty("Name", out _).Should().BeTrue();
            errors.TryGetProperty("Description", out _).Should().BeTrue();
            errors.TryGetProperty("Category", out _).Should().BeTrue();
            errors.TryGetProperty("UnitOfMeasure", out _).Should().BeTrue();
        }

        [Fact]
        public async Task CreateProduct_WithDuplicateSku_ReturnsUnprocessableEntity()
        {
            var firstResponse = await Client.PostAsJsonAsync("/api/v1/products", ProductData.Product1, CancellationToken.None);
            firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var duplicateResponse = await Client.PostAsJsonAsync("/api/v1/products", ProductData.Product1, CancellationToken.None);

            duplicateResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            using var document = await duplicateResponse.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            document!.RootElement.GetProperty("errors").TryGetProperty("SKU", out _).Should().BeTrue();
        }

        [Fact]
        public async Task CreateProduct_WithInvalidToken_ReturnsUnauthorized()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/products")
            {
                Content = JsonContent.Create(ProductData.Product1)
            };
            request.Headers.Authorization = new("Bearer", TestAuthHandler.InvalidToken);

            var response = await Client.SendAsync(request, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
    