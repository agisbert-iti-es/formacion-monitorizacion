using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApplication2.Middleware;

var builder = WebApplication.CreateBuilder(args);

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

// Request/Response logging middleware
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.MapControllers();

app.Run();
