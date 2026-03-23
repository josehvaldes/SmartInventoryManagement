using Asp.Versioning;
using FluentValidation;
using MediatR;

using Serilog;
using SmartInventory.API.Mappings;
using SmartInventory.API.Middleware;
using SmartInventory.Infrastructure;
using SmartInventory.Application;
using SmartInventory.API;

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

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

builder.Services.AddAPIDependencies(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseSerilogRequestLogging();     // Enable request logging

if (app.Environment.IsDevelopment())
{
    // Serve each version at its own URL
    app.MapOpenApi("/openapi/{documentName}.json");
}

app.MapCustomBehaviors(); // Extension method for any custom middleware or behaviors

app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
