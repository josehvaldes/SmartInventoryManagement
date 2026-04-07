using Microsoft.EntityFrameworkCore;
using Serilog;
using SmartInventory.API;
using SmartInventory.API.Mappings;
using SmartInventory.API.Middleware;
using SmartInventory.Application;
using SmartInventory.Infrastructure;
using SmartInventory.Infrastructure.AWS;
using SmartInventory.Infrastructure.Data.Context;
using SmartInventory.Seeds;

MappingConfig.RegisterMappings();

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Logging.ClearProviders();   // Avoid duplicate logs

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();          // Register Serilog

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

builder.Services.AddAPIDependencies(builder.Configuration);
builder.Services.AddAWSDependencies(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseSerilogRequestLogging();     // Enable request logging

app.MapCustomBehaviors(); // Extension method for any custom middleware or behaviors

app.UseRateLimiter();
if (app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment()) 
{
    using var scope = app.Services.CreateScope();
    var inventoryDb = scope.ServiceProvider.GetRequiredService<SmartInventoryDbContext>();
    var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

    await inventoryDb.Database.MigrateAsync();
    await authDb.Database.MigrateAsync();
    var connectionString = builder.Configuration.GetConnectionString("SmartInventoryDb") ?? string.Empty;
    await DatabaseSeeder.SeedAllAsync(connectionString);
}

app.Run();
