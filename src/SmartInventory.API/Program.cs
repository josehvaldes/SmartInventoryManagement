using Microsoft.EntityFrameworkCore;
using SmartInventory.API.Mappings;
using SmartInventory.API.Middleware;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Products.Queries.GetProducts;
using SmartInventory.Infrastructure.Data.Context;
MappingConfig.RegisterMappings();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SmartInventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Expose SmartInventoryDbContext as IApplicationDbContext so handlers can inject it
builder.Services.AddScoped<IApplicationDbContext>(
    provider => provider.GetRequiredService<SmartInventoryDbContext>());

// Expose AuthDbContext as IAuthDbContext so auth handlers can inject it
builder.Services.AddScoped<IAuthDbContext>(
    provider => provider.GetRequiredService<AuthDbContext>());

// Scan the Application assembly where all handlers live
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetProductsQuery).Assembly));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
