using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartInventory.API.Services;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data.Context;
using Xunit;

namespace SmartInventory.IntegrationTests.Infrastructure
{
    public abstract class IntegrationTest(IntegrationTestFixture fixture) : IAsyncLifetime
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        protected readonly HttpClient Client = fixture.Client;

        // SqlConnection.ConnectionString drops the password once the connection is opened, so use this instead.
        protected string ConnectionString => fixture.ConnectionString;
        protected string RedisConnectionString => fixture.RedisConnectionString;

        protected SmartInventoryDbContext CreateSmartInventoryDbContext(bool isTenantScoped = true)
        {
            var options = new DbContextOptionsBuilder<SmartInventoryDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;
            var tenantContext = new TenantContext();
            tenantContext.Initialize(SystemTenants.LegacyTenantId, "Admin", isTenantScoped);
            return new SmartInventoryDbContext(options, tenantContext);
        }

        protected AuthDbContext CreateAuthDbContext()
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;
            return new AuthDbContext(options);
        }

        public async ValueTask InitializeAsync()
        {
            
            using var connection = new SqlConnection(fixture.ConnectionString);
            await connection.OpenAsync();
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText = @"
    SET QUOTED_IDENTIFIER ON;

    DECLARE @sql NVARCHAR(MAX);

    -- Disable all foreign key constraints
    DECLARE @disableSql NVARCHAR(MAX) = N'';

    SELECT @disableSql +=
        N'ALTER TABLE ' +
        QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' +
        QUOTENAME(t.name) +
        N' NOCHECK CONSTRAINT ALL;'
    from sys.tables t join sys.schemas s on t.schema_id = s.schema_id
        where s.name <> 'Auth' AND t.name <> '__EFMigrationsHistory';

    EXEC sp_executesql @disableSql;


    -- Delete all rows
    DECLARE @deleteSql NVARCHAR(MAX) = N'';

    SELECT @deleteSql +=
        N'DELETE FROM ' +
        QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' +
        QUOTENAME(t.name) + N';'
    from sys.tables t join sys.schemas s on t.schema_id = s.schema_id
        where s.name <> 'Auth' AND t.name <> '__EFMigrationsHistory';


    EXEC sp_executesql @deleteSql;


    -- Re-enable and validate constraints
    DECLARE @enableSql NVARCHAR(MAX) = N'';

    SELECT @enableSql +=
        N'ALTER TABLE ' +
        QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' +
        QUOTENAME(t.name) +
        N' WITH CHECK CHECK CONSTRAINT ALL;'
    from sys.tables t join sys.schemas s on t.schema_id = s.schema_id
        where s.name <> 'Auth' AND t.name <> '__EFMigrationsHistory';

    EXEC sp_executesql @enableSql;
            ";
            await command.ExecuteNonQueryAsync();
            await transaction.CommitAsync();

            Client.DefaultRequestHeaders.Clear();
            await SeedDataAsync(connection);
        }

        protected abstract Task SeedDataAsync(SqlConnection connection);
    }
}
