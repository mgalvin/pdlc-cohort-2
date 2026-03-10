using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace Todo.Api.Tests;

public class HealthEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public HealthEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthLive_ReturnsHealthy()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/live");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(body);
        Assert.Equal("Healthy", json.GetProperty("status").GetString());
    }

    [Fact]
    public async Task HealthReady_WithHealthyDependencies_ReturnsHealthy()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/ready");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(body);
        Assert.Equal("Healthy", json.GetProperty("status").GetString());
    }

    [Fact]
    public async Task HealthReady_WithUnhealthyDependencies_ReturnsServiceUnavailable()
    {
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddHealthChecks()
                    .AddCheck("unhealthy", () => HealthCheckResult.Unhealthy("test"), tags: new[] { "ready" });
            });
        });
        var client = factory.CreateClient();
        var response = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task HealthDetailed_ReturnsComponentLevelDetails()
    {
        // TestWebApplicationFactory enables TestAuth so requests are treated as authenticated
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
        Assert.True(json.TryGetProperty("status", out _));
        Assert.True(json.TryGetProperty("totalDuration", out _));
        Assert.True(json.TryGetProperty("checks", out var checks));
        Assert.True(checks.EnumerateObject().Any());
    }

    [Fact]
    public async Task HealthDetailed_WhenNotAuthenticated_Returns401()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPingV1_Returns200AndPong()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/ping");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("pong", body);
    }

    [Fact]
    public async Task GetRoot_Returns200()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Response_IncludesCorrelationIdHeader()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/live");
        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.TryGetValues("X-Correlation-ID", out var values) && values.Any());
    }
}

/// <summary>
/// Web application factory that enables test auth so authorization-protected endpoints (e.g. /health) can be exercised.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("TestAuth:Enabled", "true");
    }
}
