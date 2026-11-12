using Amazon.Runtime;
using Amazon.S3;
using Docker.DotNet.Models;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.LocalStack;
using Testcontainers.MsSql;
using Testcontainers.Redis;
using Xunit;

namespace SmartInventory.IntegrationTests.Infrastructure
{
    public sealed class IntegrationTestFixture : IAsyncLifetime
    {
        private readonly MsSqlContainer _database = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04")
            .WithDatabase("SmartInventoryDB")
            .Build();

        private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7.2-alpine")
                .Build();

        private readonly LocalStackContainer _localStackContainer = new LocalStackBuilder("localstack/localstack:4.14")
            .Build();

        private SmartInventoryWebApplicationFactory _factory = null!;

        public const string S3BucketName = "smart-inventory-test-bucket";

        public string ConnectionString => _database.GetConnectionString();
        public string RedisConnectionString => _redisContainer.GetConnectionString();
        public string S3ServiceUrl => _localStackContainer.GetConnectionString();

        public HttpClient Client { get; private set; } = null!;
        public async ValueTask InitializeAsync()
        {
            await _database.StartAsync();
            await _redisContainer.StartAsync();
            await _localStackContainer.StartAsync();

            await InitializeDatabase();
            await CreateS3BucketAsync();
            // Use a test database connection string
            _factory = new SmartInventoryWebApplicationFactory(ConnectionString, RedisConnectionString, S3ServiceUrl, S3BucketName);
            Client = _factory.CreateClient();


        }


        public async ValueTask DisposeAsync()
        {
            // Cleanup resources if needed
            await _factory.DisposeAsync();
            await _database.StopAsync();
        }

        private async Task CreateS3BucketAsync()
        {
            using var s3 = new AmazonS3Client(
                new BasicAWSCredentials("test", "test"),
                new AmazonS3Config
                {
                    ServiceURL = _localStackContainer.GetConnectionString(),
                    AuthenticationRegion = "us-east-1",
                    ForcePathStyle = true
                });
            await s3.PutBucketAsync(S3BucketName);
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

            var authOptions = new DbContextOptionsBuilder<AuthDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;
            using (var context = new AuthDbContext(authOptions))
            {
                await context.Database.MigrateAsync();
            }
        }
    }
}
