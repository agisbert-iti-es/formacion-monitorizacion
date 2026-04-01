using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace MyDotNetApi.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{     

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

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError("Something went wrong: {Exception}", exception.ToString());
        await HandleExceptionAsync(httpContext, exception);
        return true;
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