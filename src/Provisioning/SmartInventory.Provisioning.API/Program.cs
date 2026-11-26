using Serilog;
using SmartInventory.Provisioning.API;
using SmartInventory.Provisioning.API.Configuration;
using SmartInventory.Provisioning.API.Endpoints;
using SmartInventory.Provisioning.API.Logging;
using SmartInventory.Provisioning.API.Middleware;
using SmartInventory.Provisioning.API.Settings;
using SmartInventory.Provisioning.Observability;

var builder = WebApplication.CreateBuilder(args);


// Configure Serilog
builder.Logging.ClearProviders();   // Avoid duplicate logs

var appSettings = builder.Configuration.GetSection(AppSettings.SectionName)
                                    .Get<AppSettings>() ?? new AppSettings();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("version", appSettings.AppVersion)
    .Enrich.WithProperty("service", appSettings.AppName)
    .Enrich.With<OpenTelemetryEnricher>()
    .WriteTo.Console(new PythonCompatibleJsonFormatter(appSettings.AppName, appSettings.AppVersion))
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpPipeline();

builder.Services.AddOpenApi();
builder.Services.AddApiDepedencies(builder.Configuration);

var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseSerilogRequestLogging();
app.UseRateLimiter();

app.UseAuthentication(); //  Add authentication middleware
app.UseAuthorization();

app.MapApiEndpoints(); // Map the endpoints
app.AddPrometheusMapping(); // Add Prometheus metrics mapping

app.Run();
