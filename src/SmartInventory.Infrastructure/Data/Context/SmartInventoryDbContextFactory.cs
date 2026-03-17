using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartInventory.Infrastructure.Data.Context
{
    public class SmartInventoryDbContextFactory : IDesignTimeDbContextFactory<SmartInventoryDbContext>
    {
        public SmartInventoryDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SmartInventoryDbContext>();

            optionsBuilder.UseSqlServer(
                $"Server={FactorySettings.SERVER_NAME};Database={FactorySettings.DATABASE_NAME};Trusted_Connection=True;TrustServerCertificate=True;"
            );

            return new SmartInventoryDbContext(optionsBuilder.Options);
        }
    }
}
