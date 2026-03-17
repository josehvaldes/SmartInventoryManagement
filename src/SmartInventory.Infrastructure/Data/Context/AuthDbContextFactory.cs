using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartInventory.Infrastructure.Data.Context
{
    public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
    {
        public AuthDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();

            optionsBuilder.UseSqlServer(
                $"Server={FactorySettings.SERVER_NAME};Database={FactorySettings.DATABASE_NAME};Trusted_Connection=True;TrustServerCertificate=True;"
            );

            return new AuthDbContext(optionsBuilder.Options);
        }
    }
}
