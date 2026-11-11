using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
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
        QUOTENAME(SCHEMA_NAME(schema_id)) + N'.' +
        QUOTENAME(name) +
        N' NOCHECK CONSTRAINT ALL;'
    FROM sys.tables;

    EXEC sp_executesql @disableSql;


    -- Delete all rows
    DECLARE @deleteSql NVARCHAR(MAX) = N'';

    SELECT @deleteSql +=
        N'DELETE FROM ' +
        QUOTENAME(SCHEMA_NAME(schema_id)) + N'.' +
        QUOTENAME(name) + N';'
    FROM sys.tables;

    EXEC sp_executesql @deleteSql;


    -- Re-enable and validate constraints
    DECLARE @enableSql NVARCHAR(MAX) = N'';

    SELECT @enableSql +=
        N'ALTER TABLE ' +
        QUOTENAME(SCHEMA_NAME(schema_id)) + N'.' +
        QUOTENAME(name) +
        N' WITH CHECK CHECK CONSTRAINT ALL;'
    FROM sys.tables;

    EXEC sp_executesql @enableSql;
            ";
            await command.ExecuteNonQueryAsync();
            await transaction.CommitAsync();

            await SeedDataAsync(connection);
        }

        protected abstract Task SeedDataAsync(SqlConnection connection);
    }
}
