using Docker.DotNet.Models;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.MsSql;
using Testcontainers.Redis;
using Xunit;

namespace SmartInventory.IntegrationTests.Infrastructure
{
    public sealed class IntegrationTestFixture : IAsyncLifetime
    {
        private readonly MsSqlContainer _database = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04")
            .Build();

        private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7.2-alpine")
                .Build();

        private SmartInventoryWebApplicationFactory _factory = null!;

        public string ConnectionString => _database.GetConnectionString();
        public string RedisConnectionString => _redisContainer.GetConnectionString();

        public HttpClient Client { get; private set; } = null!;
        public async ValueTask InitializeAsync()
        {
            await _database.StartAsync();
            await _redisContainer.StartAsync();

            await InitializeDatabase();
            // Use a test database connection string
            _factory = new SmartInventoryWebApplicationFactory(ConnectionString, RedisConnectionString);
            Client = _factory.CreateClient();


        }


        public async ValueTask DisposeAsync()
        {
            // Cleanup resources if needed
            await _factory.DisposeAsync();
            await _database.StopAsync();
        }

        private async Task InitializeDatabase() 
        {
            var options = new DbContextOptionsBuilder<SmartInventoryDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;
            using (var context = new SmartInventoryDbContext(options))
            {
                await context.Database.MigrateAsync();
            }
        }
    }
}
