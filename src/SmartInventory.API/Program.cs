using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SmartInventory.API.Mappings;
using SmartInventory.API.Middleware;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Common.Validation;
using SmartInventory.Application.Features.Products.Queries.GetProducts;
using SmartInventory.Infrastructure.Data.Context;

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
// One OpenAPI document per version — add "v2" here when v2 is created
builder.Services.AddOpenApi("v1");


builder.Services.AddApiVersioning(options => { 
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),   // /api/v1/products
        new HeaderApiVersionReader("X-Api-Version")); // optional fallback
}).AddApiExplorer( options => { 
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

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
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(GetProductsQuery).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.LicenseKey = builder.Configuration["MediatR:LicenseKey"] ?? "FREE-LIMITED-USE";

});

builder.Services.AddValidatorsFromAssembly(typeof(ValidationBehavior<,>).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseSerilogRequestLogging();     // Enable request logging

if (app.Environment.IsDevelopment())
{
    // Serve each version at its own URL
    app.MapOpenApi("/openapi/{documentName}.json");
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
