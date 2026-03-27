using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApplication2.Middleware;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Pyroscope;
var builder = WebApplication.CreateBuilder(args);

// ==========================================
// CONFIGURACIÓN DE OPENTELEMETRY Y PYROSCOPE
// ==========================================

Pyroscope.Profiler.Instance.SetCPUTrackingEnabled(true);
Pyroscope.Profiler.Instance.SetAllocationTrackingEnabled(true); // Memoria
Pyroscope.Profiler.Instance.SetContentionTrackingEnabled(true); // Hilos bloqueados

Console.WriteLine("Pyroscope instrumentado correctamente desde el código.");


var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(builder.Environment.ApplicationName ?? "my-dotnet-api");

// 1. Redirigir los Logs nativos de .NET (ILogger) a OpenTelemetry
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.SetResourceBuilder(resourceBuilder);
    logging.AddOtlpExporter();
});

// 2. Configurar Trazas y Métricas
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation(options =>
                {
                    // ESTO ES CLAVE: Registra el objeto de excepción completo
                    options.RecordException = true; 
                })
               .AddAspNetCoreInstrumentation() // Captura peticiones HTTP entrantes
               .AddHttpClientInstrumentation() // Captura peticiones HTTP salientes
               .AddEntityFrameworkCoreInstrumentation() // Captura las queries SQL a SQLite
               .AddOtlpExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.SetResourceBuilder(resourceBuilder)
               .AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddRuntimeInstrumentation() // CPU, Memoria, Garbage Collector
               .AddOtlpExporter();
    });

    
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Register schema filter to add example JSON for models
    c.SchemaFilter<WebApplication2.Swagger.SchemaExamples>();
});
// Health checks
builder.Services.AddHealthChecks();

// Register services
builder.Services.AddScoped<WebApplication2.Repositories.IPlayerRepository, WebApplication2.Repositories.PlayerRepository>();
builder.Services.AddScoped<WebApplication2.Services.IPlayerService, WebApplication2.Services.PlayerService>();

// Configure DbContext for SQLite
builder.Services.AddDbContext<WebApplication2.Data.AppDbContext>(options =>
    options.UseSqlite("Data Source=players.db"));

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WebApplication2.Data.AppDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
// Enable Swagger and make the UI available at the application root (/)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication2 API V1");
    c.RoutePrefix = string.Empty; // serve Swagger UI at '/'
});

// Only enable HTTPS redirection when an HTTPS URL is configured (avoids redirect warnings in containers)
var configuredUrls = builder.Configuration["ASPNETCORE_URLS"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
var enableHttpsRedirect = !string.IsNullOrEmpty(configuredUrls) && configuredUrls.Split(';').Any(u => u.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
if (enableHttpsRedirect)
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

// Global exception handler middleware
app.UseMiddleware<WebApplication2.Middleware.GlobalExceptionHandler>();

// Request/Response logging middleware
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.MapControllers();

app.Run();
