using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Todo.Core.Infrastructure.Health;

/// <summary>
/// Writes health check responses in consistent JSON format per Backend blueprint.
/// </summary>
public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Writes a simple health response (overall status only) for liveness and readiness endpoints.
    /// </summary>
    public static Task WriteSimpleHealthResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var status = report.Status.ToString();
        var payload = new { status };
        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }

    /// <summary>
    /// Writes a detailed health response with status, totalDuration, and per-check details.
    /// </summary>
    public static Task WriteDetailedHealthResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var checks = report.Entries.ToDictionary(
            e => e.Key,
            e => new
            {
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                data = ToJsonSafeDictionary(e.Value.Data),
                exception = e.Value.Exception?.Message
            });

        var payload = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks
        };
        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }

    private static IReadOnlyDictionary<string, object>? ToJsonSafeDictionary(IReadOnlyDictionary<string, object>? data)
    {
        if (data == null || data.Count == 0) return data;
        var result = new Dictionary<string, object>(data.Count);
        foreach (var kv in data)
        {
            result[kv.Key] = kv.Value is string or int or long or double or bool
                ? kv.Value
                : (object)(kv.Value?.ToString() ?? "");
        }
        return result;
    }
}
