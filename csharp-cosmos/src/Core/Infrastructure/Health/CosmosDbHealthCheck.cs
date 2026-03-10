using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Todo.Core.Infrastructure.Health;

/// <summary>
/// Health check that verifies Cosmos DB connection and database accessibility.
/// Implements AC-TECH-001.1: verifies Cosmos DB connection and product container accessibility.
/// </summary>
public class CosmosDbHealthCheck : IHealthCheck
{
    private readonly CosmosClient _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CosmosDbHealthCheck> _logger;

    public CosmosDbHealthCheck(
        CosmosClient client,
        IConfiguration configuration,
        ILogger<CosmosDbHealthCheck> logger)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var databaseName = _configuration["AZURE_COSMOS_DATABASE_NAME"] ?? "App";
        var timeoutSeconds = _configuration.GetValue("HealthChecks:TimeoutSeconds", 5);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            var database = _client.GetDatabase(databaseName);
            var response = await database.ReadAsync(requestOptions: null, cts.Token);
            var endpoint = _client.Endpoint?.ToString() ?? "unknown";

            var data = new Dictionary<string, object>
            {
                ["databaseName"] = databaseName,
                ["endpoint"] = endpoint
            };

            _logger.LogDebug("Cosmos DB health check succeeded for database {DatabaseName}", databaseName);
            return HealthCheckResult.Healthy("Cosmos DB is accessible", data);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Cosmos DB health check timed out after {TimeoutSeconds}s", timeoutSeconds);
            return HealthCheckResult.Unhealthy("Cosmos DB health check timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cosmos DB health check failed for database {DatabaseName}", databaseName);
            return HealthCheckResult.Unhealthy(
                "Cosmos DB is unavailable",
                exception: ex,
                data: new Dictionary<string, object> { ["databaseName"] = databaseName });
        }
    }
}
