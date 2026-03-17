
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.DTOs;
using SmartInventory.Infrastructure.Data.Context;
using Xunit;
using Mapster;

namespace SmartInventory.IntegrationTests
{
    public class MappingIntegrationTests
    {

        [Fact]
        public void ProjectTo_Mapping_IsValid()
        {
            // This test would require a mock of the database context and is typically done in integration tests rather than unit tests.
            var connectionString = "Server=PEPUSOPC\\MSSQLSERVER01;Database=smart_inventory;Trusted_Connection=True;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<SmartInventoryDbContext>()
                            .UseSqlServer(connectionString)
                            .Options;
            using (var context = new SmartInventoryDbContext(options))
            {
                var list = context.Products.ProjectToType<ProductDto>().ToList();
                var productDto = list.FirstOrDefault();
                Assert.NotNull(productDto);
                Assert.Equal("USB-C Charging Cable 2m", productDto.Name);
            }
        }
    }
}
