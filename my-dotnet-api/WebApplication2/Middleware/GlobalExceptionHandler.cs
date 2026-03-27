using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WebApplication2.Middleware;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        var responseModel = new ErrorResponse
        {
            Timestamp = DateTime.UtcNow,
            Path = context.Request.Path,
            Method = context.Request.Method
        };

        responseModel.Error = exception switch
        {
            // Manejo de excepciones específicas
            ArgumentException => new ErrorDetails
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "Bad Request",
                Details = exception.Message
            },
            KeyNotFoundException => new ErrorDetails
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = "Not Found",
                Details = exception.Message
            },
            _ => new ErrorDetails
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "Internal Server Error",
                Details = "An internal server error occurred"
            }
        };

        response.StatusCode = responseModel.Error.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(responseModel);
        await response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public DateTime Timestamp { get; set; }
    public string? Path { get; set; }
    public string? Method { get; set; }
    public ErrorDetails? Error { get; set; }
}

public class ErrorDetails
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
}