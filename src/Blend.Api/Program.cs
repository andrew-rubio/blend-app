using System.Text;
using System.Threading.RateLimiting;
using Blend.Api.Configuration;
using Blend.Api.Domain;
using Blend.Api.Identity;
using Blend.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
var authSettings = builder.Configuration.GetSection("Auth").Get<AuthSettings>() ?? new AuthSettings();
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" };

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));

// Repositories & Services
builder.Services.AddSingleton<ICosmosUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddTransient<IEmailService, DevEmailService>();

// Identity
builder.Services.AddIdentityCore<BlendUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<BlendRole>()
.AddUserStore<CosmosUserStore>()
.AddRoleStore<CosmosRoleStore>()
.AddDefaultTokenProviders()
.AddSignInManager();

// Authentication - single call, chain all providers
var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

if (!string.IsNullOrEmpty(authSettings.Google.ClientId))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = authSettings.Google.ClientId;
        options.ClientSecret = authSettings.Google.ClientSecret;
    });
}

if (!string.IsNullOrEmpty(authSettings.Facebook.AppId))
{
    authBuilder.AddFacebook(options =>
    {
        options.AppId = authSettings.Facebook.AppId;
        options.AppSecret = authSettings.Facebook.AppSecret;
    });
}
if (!string.IsNullOrEmpty(authSettings.Twitter.ConsumerKey))
{
    authBuilder.AddTwitter(options =>
    {
        options.ConsumerKey = authSettings.Twitter.ConsumerKey;
        options.ConsumerSecret = authSettings.Twitter.ConsumerSecret;
    });
}

// Authorization
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAuthenticated", policy => policy.RequireAuthenticatedUser())
    .AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("anonymous", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddPolicy("authenticated", httpContext =>
    {
        var userId = httpContext.User.Identity?.IsAuthenticated == true
            ? httpContext.User.FindFirst("sub")?.Value ?? "anon"
            : httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return httpContext.User.IsInRole("Admin")
            ? RateLimitPartition.GetNoLimiter<string>("admin")
            : RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// MVC / API
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Blend API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
