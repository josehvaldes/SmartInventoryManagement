var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.SmartInventory_API>("api");

//var sql = builder.AddConnectionString("SmartInventoryDb");
var sql = builder.AddSqlServer("sql")
                 .AddDatabase("SmartInventoryDb");

var redis = builder.AddRedis("redis");

api.WithReference(redis).WithReference(sql).WaitFor(sql);

builder.Build().Run();
