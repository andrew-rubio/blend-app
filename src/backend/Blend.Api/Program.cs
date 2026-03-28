using Blend.Api.Account.Services;
using Blend.Api.Admin.Services;
using Blend.Api.Auth;
using Blend.Api.CookSessions.Services;
using Blend.Api.Friends.Services;
using Blend.Api.Home.Services;
using Blend.Api.Ingredients.Services;
using Blend.Api.Middleware;
using Blend.Api.Notifications.Services;
using Blend.Api.Preferences.Services;
using Blend.Api.Profile.Services;
using Blend.Api.Recipes.Services;
using Blend.Api.Search.Services;
using Blend.Api.Services.Spoonacular;
using Blend.Infrastructure.BlobStorage;
using Blend.Infrastructure.Cosmos;
using Azure.Identity;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// ── Azure Key Vault Configuration ────────────────────────────────────────────
// Load secrets from Key Vault into the configuration system so they're available
// as normal configuration values (e.g. Jwt:SecretKey, Spoonacular:ApiKey).
// Key Vault secret names use "--" as separator, which maps to ":" in config keys.
var keyVaultUri = builder.Configuration["KeyVault:Uri"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new DefaultAzureCredential());
}

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
// We only add the "ready" checks here for the readiness endpoint.
var healthBuilder = builder.Services.AddHealthChecks()
    .AddCheck("ready", () => HealthCheckResult.Healthy("API is ready to serve requests"), ["ready"]);

// Cosmos DB health check (only when configured)
if (!string.IsNullOrWhiteSpace(builder.Configuration.GetSection("CosmosDb")["ConnectionString"])
    || !string.IsNullOrWhiteSpace(builder.Configuration.GetSection("CosmosDb")["EndpointUri"]))
{
    healthBuilder.AddCheck("cosmosDb",
        () => HealthCheckResult.Healthy("Cosmos DB is reachable"),
        ["ready", "cosmosDb"]);
}
else
{
    healthBuilder.AddCheck("cosmosDb",
        () => HealthCheckResult.Degraded("Cosmos DB is not configured"),
        ["ready", "cosmosDb"]);
}

// Spoonacular health check (only when configured)
if (!string.IsNullOrWhiteSpace(builder.Configuration["Spoonacular:ApiKey"]))
{
    healthBuilder.AddCheck("spoonacular",
        () => HealthCheckResult.Healthy("Spoonacular is configured"),
        ["ready", "spoonacular"]);
}
else
{
    healthBuilder.AddCheck("spoonacular",
        () => HealthCheckResult.Degraded("Spoonacular is not configured — search will use internal results only"),
        ["ready", "spoonacular"]);
}

// Knowledge Base health check (only when configured)
if (!string.IsNullOrWhiteSpace(builder.Configuration["IngredientSearch:Endpoint"]))
{
    healthBuilder.AddCheck("knowledgeBase",
        () => HealthCheckResult.Healthy("Knowledge Base is configured"),
        ["ready", "knowledgeBase"]);
}
else
{
    healthBuilder.AddCheck("knowledgeBase",
        () => HealthCheckResult.Degraded("Knowledge Base is not configured — smart suggestions will be disabled"),
        ["ready", "knowledgeBase"]);
}

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

// ── Azure Blob Storage & Image Processing ─────────────────────────────────────
// Register when a connection string is available (Aspire-injected or manual config).
var blobConnectionString =
    builder.Configuration.GetConnectionString("blobs")
    ?? builder.Configuration["AzureBlobStorage:ConnectionString"];

if (!string.IsNullOrWhiteSpace(blobConnectionString))
{
    builder.Services.AddBlobStorage(builder.Configuration);
}

// ── Preferences ───────────────────────────────────────────────────────────────
builder.Services.AddPreferenceServices();

// ── Recipes ───────────────────────────────────────────────────────────────────
builder.Services.AddRecipeServices();

// ── Profile ────────────────────────────────────────────────────────────────────
builder.Services.AddProfileServices();

// ── Search ────────────────────────────────────────────────────────────────────
builder.Services.AddSearchServices();

// ── Ingredient Knowledge Base ─────────────────────────────────────────────────
builder.Services.AddKnowledgeBaseServices(builder.Configuration);

// ── Cook Mode Sessions ────────────────────────────────────────────────────────
builder.Services.AddCookSessionServices();

// ── Friends System ─────────────────────────────────────────────────────────────
builder.Services.AddFriendsServices();

// ── Home Page ─────────────────────────────────────────────────────────────────
builder.Services.AddHomeServices();

// ── Notifications ─────────────────────────────────────────────────────────────
builder.Services.AddNotificationServices();

// ── Account Management ─────────────────────────────────────────────────────────
builder.Services.AddAccountServices();

// ── Admin Content Management ───────────────────────────────────────────────────
builder.Services.AddAdminServices();

// ── Routing ────────────────────────────────────────────────────────────────────
builder.Services.AddRouting(options => options.LowercaseUrls = true);

var app = builder.Build();

// ── Middleware Pipeline ────────────────────────────────────────────────────────
app.UseCorrelationId();
app.UseGlobalExceptionHandler();
app.UseRequestLogging();

// ── Security Headers ──────────────────────────────────────────────────────────
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    }
    await next();
});

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
        // In production, only expose aggregate status (no per-service detail to prevent info disclosure).
        if (app.Environment.IsProduction())
        {
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow,
            });
            await context.Response.WriteAsync(result);
        }
        else
        {
            var services = report.Entries.ToDictionary(
                e => e.Key,
                e => new { status = e.Value.Status.ToString(), description = e.Value.Description });
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow,
                services
            });
            await context.Response.WriteAsync(result);
        }
    }
});

// ── API Routes ─────────────────────────────────────────────────────────────────
app.MapControllers();

app.MapDefaultEndpoints();

app.Run();

// Make Program accessible to integration tests
public partial class Program { }
