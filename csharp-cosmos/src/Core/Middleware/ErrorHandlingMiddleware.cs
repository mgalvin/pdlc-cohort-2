using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Todo.Core.Middleware;

/// <summary>
/// Global error handling middleware. Catches exceptions, logs with Serilog, returns consistent error response per Backend blueprint.
/// </summary>
public class ErrorHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;

    public ErrorHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, code, message) = MapException(ex);
        var correlationId = context.TraceIdentifier;

        Log.Error(ex, "Unhandled exception. {StatusCode} {Code} {CorrelationId}",
            (int)statusCode, code, correlationId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var errorDetail = new { code, message, field = (string?)null };
        var payload = new { errors = new[] { errorDetail } };

        if (_env.IsDevelopment())
        {
            var devError = new { code, message, field = (string?)null, detail = ex.ToString() };
            payload = new { errors = new[] { devError } };
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }

    private static (HttpStatusCode statusCode, string code, string message) MapException(Exception ex)
    {
        return ex switch
        {
            KeyNotFoundException => (HttpStatusCode.NotFound, "NOT_FOUND", ex.Message),
            ArgumentException => (HttpStatusCode.BadRequest, "BAD_REQUEST", ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "UNAUTHORIZED", ex.Message),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred.")
        };
    }
}
