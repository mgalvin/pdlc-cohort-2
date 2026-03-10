using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Todo.Api.Infrastructure;
using Todo.Core.Infrastructure;
using Todo.Core.Infrastructure.Health;
using Todo.Core.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Serilog as the logging provider
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext());

// Core infrastructure (Cosmos and shared registrations via extensions)
builder.Services.AddCoreInfrastructure(builder.Configuration);

builder.Services.AddCors();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(TestAuthHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

// Middleware order: request logging first, then CORS, then error handling, then routing/endpoints
app.UseRequestLogging();
app.UseCors(policy =>
{
    policy.AllowAnyOrigin();
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
});
app.UseErrorHandling();

// Swagger UI
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("./openapi.yaml", "v1");
    options.RoutePrefix = "";
});

app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = HealthCheckResponseWriter.WriteSimpleHealthResponse
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteSimpleHealthResponse,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("detailed"),
    ResponseWriter = HealthCheckResponseWriter.WriteDetailedHealthResponse
}).RequireAuthorization();
app.MapGet("/", () => Results.Ok("OK"));

app.Run();
