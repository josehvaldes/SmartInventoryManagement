using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartInventory.Contracts.Responses;
using SmartInventory.Contracts.Responses.Products;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;
using SmartInventory.Infrastructure.Data.Context;
using SmartInventory.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SmartInventory.IntegrationTests.Products
{
    [Collection("Integration")]
    public class GetProductsTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
    {
        protected override async Task SeedDataAsync(SqlConnection connection)
        {
            var options = new DbContextOptionsBuilder<SmartInventoryDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;

            await using var context = new SmartInventoryDbContext(options);

            var products = ProductData.Products;

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        private async Task<PagedResponse<ProductResponse>> GetPage(string query)
        {
            var response = await Client.GetAsync($"/api/v1/products?{query}", CancellationToken.None);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var page = await response.Content.ReadFromJsonAsync<PagedResponse<ProductResponse>>(cancellationToken: CancellationToken.None);
            page.Should().NotBeNull();

            return page!;
        }

        private async Task<IReadOnlyList<ProductResponse>> GetProducts(string query)
            => (await GetPage(query)).Items;

        [Fact]
        public async Task GetProducts_ReturnsExpectedProducts()
        {
            // Arrange
            var query = "pageNumber=1&pageSize=10";

            // Act
            var products = await GetProducts(query);

            // Assert
            products.Should().HaveCount(2);
            products.Select(p => p.SKU).Should().Contain(["ITM-USB-C-001", "ITM-GLOVES-001"]);
        }

        [Fact]
        public async Task GetProducts_WithoutPagingParameters_UsesDefaultPaging()
        {
            // Act
            var page = await GetPage(string.Empty);

            // Assert
            page.PageNumber.Should().Be(1);
            page.PageSize.Should().Be(20);
            page.TotalCount.Should().Be(2);
            page.TotalPages.Should().Be(1);
            page.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetProducts_ReturnsPagingMetadata()
        {
            // Act
            var page = await GetPage("pageNumber=1&pageSize=10");

            // Assert
            page.TotalCount.Should().Be(2);
            page.PageNumber.Should().Be(1);
            page.PageSize.Should().Be(10);
            page.TotalPages.Should().Be(1);
            page.HasPreviousPage.Should().BeFalse();
            page.HasNextPage.Should().BeFalse();
        }

        [Fact]
        public async Task GetProducts_ReturnsMappedProductDetails()
        {
            // Act
            var products = await GetProducts("pageNumber=1&pageSize=10");

            // Assert
            var cable = products.Single(p => p.SKU == "ITM-USB-C-001");
            cable.Id.Should().NotBeEmpty();
            cable.Name.Should().Be("USB-C Charging Cable 2m");
            cable.Description.Should().Be("Braided USB-C cable for daily use");
            cable.Category.Should().Be(nameof(ProductCategory.Electronics));
            cable.UnitOfMeasure.Should().Be(nameof(UnitOfMeasure.Piece));
            cable.UnitCost.Should().Be(9.99m);
            cable.IsActive.Should().BeTrue();

            var gloves = products.Single(p => p.SKU == "ITM-GLOVES-001");
            gloves.Name.Should().Be("Safety Gloves");
            gloves.Category.Should().Be(nameof(ProductCategory.Safety));
            gloves.UnitOfMeasure.Should().Be(nameof(UnitOfMeasure.Box));
            gloves.UnitCost.Should().Be(14.50m);
        }

        [Fact]
        public async Task GetProducts_ReturnsHateoasLinksForEachProduct()
        {
            // Act
            var products = await GetProducts("pageNumber=1&pageSize=10");

            // Assert
            foreach (var product in products)
            {
                product.Links.Select(l => l.Rel).Should().BeEquivalentTo(["self", "delete-product", "create-product"]);
                product.Links.Single(l => l.Rel == "self").Href.Should().EndWith(product.Id.ToString());
                product.Links.Should().OnlyContain(l => !string.IsNullOrEmpty(l.Href));
            }
        }

        [Fact]
        public async Task GetProducts_WithSmallPageSize_ReturnsFirstPageWithNextPage()
        {
            // Act
            var page = await GetPage("pageNumber=1&pageSize=1");

            // Assert
            page.Items.Should().HaveCount(1);
            page.TotalCount.Should().Be(2);
            page.TotalPages.Should().Be(2);
            page.HasPreviousPage.Should().BeFalse();
            page.HasNextPage.Should().BeTrue();
        }

        [Fact]
        public async Task GetProducts_LastPage_HasPreviousPageAndNoNextPage()
        {
            // Act
            var page = await GetPage("pageNumber=2&pageSize=1");

            // Assert
            page.Items.Should().HaveCount(1);
            page.PageNumber.Should().Be(2);
            page.HasPreviousPage.Should().BeTrue();
            page.HasNextPage.Should().BeFalse();
        }

        [Fact]
        public async Task GetProducts_ConsecutivePages_ReturnDistinctProductsCoveringAllData()
        {
            // Act
            var first = await GetProducts("pageNumber=1&pageSize=1");
            var second = await GetProducts("pageNumber=2&pageSize=1");

            // Assert
            first.Single().Id.Should().NotBe(second.Single().Id);
            first.Concat(second).Select(p => p.SKU)
                .Should().BeEquivalentTo(["ITM-USB-C-001", "ITM-GLOVES-001"]);
        }

        [Fact]
        public async Task GetProducts_PageBeyondLastPage_ReturnsEmptyItemsWithTotalCount()
        {
            // Act
            var page = await GetPage("pageNumber=5&pageSize=10");

            // Assert
            page.Items.Should().BeEmpty();
            page.TotalCount.Should().Be(2);
            page.PageNumber.Should().Be(5);
            page.HasNextPage.Should().BeFalse();
            page.HasPreviousPage.Should().BeTrue();
        }

        [Fact]
        public async Task GetProducts_WithInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/products?pageNumber=1&pageSize=10");
            request.Headers.Authorization = new("Bearer", TestAuthHandler.InvalidToken);

            // Act
            var response = await Client.SendAsync(request, CancellationToken.None);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
