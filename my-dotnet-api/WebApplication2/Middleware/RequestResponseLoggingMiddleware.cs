using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace WebApplication2.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestResponseLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var utcNow = DateTime.UtcNow;

        // Read request body
        context.Request.EnableBuffering();
        string requestBody = string.Empty;
        if (context.Request.ContentLength > 0)
        {
            context.Request.Body.Position = 0;
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        // Capture the response
        var originalBodyStream = context.Response.Body;
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Read response body
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            string responseBodyText = string.Empty;
            using (var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true))
            {
                responseBodyText = await reader.ReadToEndAsync();
            }
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            // Prepare log entry
            var logEntry = new
            {
                timestampUtc = utcNow.ToString("o"),
                traceId = Activity.Current?.TraceId.ToString(),
                method = context.Request.Method,
                url = context.Request.Path + context.Request.QueryString,
                requestBody = string.IsNullOrEmpty(requestBody) ? null : requestBody,
                responseStatus = context.Response.StatusCode,
                responseBody = string.IsNullOrEmpty(responseBodyText) ? null : responseBodyText,
                elapsedMilliseconds = stopwatch.ElapsedMilliseconds
            };

            try
            {
                var logsDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                Directory.CreateDirectory(logsDir);
                var fileName = DateTime.UtcNow.ToString("dd-MM-yyyy") + "-my-dotnet-api.log";
                var filePath = Path.Combine(logsDir, fileName);
                var json = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = false });
                await File.AppendAllTextAsync(filePath, json + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Log the exception instead of swallowing it
                Console.WriteLine($"Error writing log: {ex.Message}");
                Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
            }

            // Copy the contents of the new memory stream (which contains the response) to the original stream.
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
    }
}
