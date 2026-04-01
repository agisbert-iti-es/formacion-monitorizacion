
using Microsoft.EntityFrameworkCore;
using MyDotNetApi.Middleware;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Pyroscope;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

ConfigureOpenTelemetry(builder);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Register schema filter to add example JSON for models
    c.SchemaFilter<MyDotNetApi.Swagger.SchemaExamples>();
});
// Health checks
builder.Services.AddHealthChecks();

builder.Services.AddProblemDetails();

// Register services
builder.Services.AddScoped<MyDotNetApi.Repositories.IPlayerRepository, MyDotNetApi.Repositories.PlayerRepository>();
builder.Services.AddScoped<MyDotNetApi.Services.IPlayerService, MyDotNetApi.Services.PlayerService>();

// Configure DbContext for SQLite
builder.Services.AddDbContext<MyDotNetApi.Data.AppDbContext>(options =>
    options.UseSqlite("Data Source=players.db"));

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyDotNetApi.Data.AppDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
// Enable Swagger and make the UI available at the application root (/)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "my_dotnet_api API V1");
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
//app.UseMiddleware<GlobalExceptionHandler>();

// Request/Response logging middleware
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseExceptionHandler();

app.MapControllers();

app.Run();

static void ConfigureOpenTelemetry(WebApplicationBuilder builder)
{
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
}