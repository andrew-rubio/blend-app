using Blend.Api.Auth;
using Blend.Api.Middleware;
using Blend.Api.Services.Spoonacular;
using Blend.Infrastructure.Cosmos;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// ── Service Defaults (OpenTelemetry, service discovery, resilience) ──────────
builder.AddServiceDefaults();

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── OpenAPI ────────────────────────────────────────────────────────────────────
builder.Services.AddOpenApi("v1");

// ── Problem Details ────────────────────────────────────────────────────────────
builder.Services.AddProblemDetails();

// ── Health Checks ──────────────────────────────────────────────────────────────
// Note: ServiceDefaults already adds a "self" check with ["live"] tag.
// We only add the "ready" check here for readiness endpoint.
builder.Services.AddHealthChecks()
    .AddCheck("ready", () => HealthCheckResult.Healthy("API is ready to serve requests"), ["ready"]);

// ── CORS ──────────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:3000"];

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

// ── Cosmos DB ─────────────────────────────────────────────────────────────────
// Only register when Cosmos DB is configured (connection string or endpoint+key present)
var cosmosSection = builder.Configuration.GetSection("CosmosDb");
if (!string.IsNullOrWhiteSpace(cosmosSection["ConnectionString"])
    || !string.IsNullOrWhiteSpace(cosmosSection["EndpointUri"]))
{
    builder.Services.AddCosmosDb(builder.Configuration);
}

// ── Authentication & Authorisation ───────────────────────────────────────────
builder.Services.AddBlendAuthentication(builder.Configuration);

// ── Spoonacular & Cache Services ─────────────────────────────────────────────
// Only register when a Spoonacular API key is configured
if (!string.IsNullOrWhiteSpace(builder.Configuration["Spoonacular:ApiKey"]))
{
    builder.Services.AddSpoonacularServices(builder.Configuration);
}

// ── Routing ────────────────────────────────────────────────────────────────────
builder.Services.AddRouting(options => options.LowercaseUrls = true);

var app = builder.Build();

// ── Middleware Pipeline ────────────────────────────────────────────────────────
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// ── Health Check Endpoints ─────────────────────────────────────────────────────
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow
        });
        await context.Response.WriteAsync(result);
    }
});

// ── API Routes ─────────────────────────────────────────────────────────────────
app.MapControllers();

app.MapDefaultEndpoints();

app.Run();

// Make Program accessible to integration tests
public partial class Program { }
