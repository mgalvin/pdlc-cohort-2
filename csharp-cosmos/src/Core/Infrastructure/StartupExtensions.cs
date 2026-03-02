using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Todo.Core.Infrastructure;

/// <summary>
/// Service registration extensions for dependency injection configuration.
/// Follows the Backend blueprint feature-module pattern for Services, Repositories, Validators.
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    /// Adds core infrastructure services: Cosmos DB client and shared registrations.
    /// Call from the API host during startup.
    /// </summary>
    public static IServiceCollection AddCoreInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCosmosDb(configuration);
        return services;
    }

    /// <summary>
    /// Registers the Cosmos DB client. Requires AZURE_COSMOS_ENDPOINT (and optionally AZURE_COSMOS_DATABASE_NAME) in configuration.
    /// </summary>
    public static IServiceCollection AddCosmosDb(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration["AZURE_COSMOS_ENDPOINT"];
        if (string.IsNullOrEmpty(endpoint))
        {
            // Allow startup without Cosmos for local dev or tests; callers must handle null client if needed.
            return services;
        }

        var credential = new DefaultAzureCredential();
        var options = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }
        };

        services.AddSingleton(_ => new CosmosClient(endpoint, credential, options));
        return services;
    }

    /// <summary>
    /// Placeholder for feature Services registration. Future features call e.g. services.AddAssetServices() from sibling extensions.
    /// </summary>
    public static IServiceCollection AddFeatureServices(this IServiceCollection services)
    {
        return services;
    }

    /// <summary>
    /// Placeholder for feature Repositories registration.
    /// </summary>
    public static IServiceCollection AddFeatureRepositories(this IServiceCollection services)
    {
        return services;
    }

    /// <summary>
    /// Placeholder for feature Validators registration.
    /// </summary>
    public static IServiceCollection AddFeatureValidators(this IServiceCollection services)
    {
        return services;
    }
}
