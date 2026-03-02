using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Todo.Core.Infrastructure;
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

builder.Services.AddHealthChecks();
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
app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = async (context, report) => await context.Response.WriteAsync("Healthy") });
app.MapGet("/", () => Results.Ok("OK"));

app.Run();
