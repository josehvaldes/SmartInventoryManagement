var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.SmartInventory_API>("api");

var sql = builder.AddConnectionString("SmartInventoryDb");
var redis = builder.AddRedis("redis");

api.WithReference(sql)
    .WithReference(redis);

builder.Build().Run();
