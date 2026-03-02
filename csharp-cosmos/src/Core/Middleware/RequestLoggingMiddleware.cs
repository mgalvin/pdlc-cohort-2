using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Todo.Core.Middleware;

/// <summary>
/// Request/response logging middleware with correlation ID and duration. Follows Backend blueprint logging standards (camelCase, correlation IDs).
/// </summary>
public class RequestLoggingMiddleware
{
    private const string CorrelationIdKey = "CorrelationId";
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");
        context.Response.Headers[CorrelationIdHeader] = correlationId;
        context.Items[CorrelationIdKey] = correlationId;

        var sw = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            Log.Information(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms. CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                correlationId);
        }
    }
}
