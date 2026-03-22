using System.Text;
using System.Threading.RateLimiting;
using Blend.Api.Auth.Services;
using Blend.Domain.Identity;
using Blend.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace Blend.Api.Auth;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddBlendAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure JWT options
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        // ASP.NET Core Identity with custom stores (no EF)
        services.AddIdentityCore<BlendUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
        })
        .AddRoles<BlendRole>()
        .AddUserStore<CosmosUserStore>()
        .AddRoleStore<CosmosRoleStore>()
        .AddDefaultTokenProviders()
        .AddSignInManager();

        // JWT Bearer + OAuth providers
        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            };
        });

        // Google OAuth (conditional)
        var googleClientId = configuration["Authentication:Google:ClientId"];
        var googleClientSecret = configuration["Authentication:Google:ClientSecret"];
        if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
            });
        }

        // Facebook OAuth (conditional)
        var facebookAppId = configuration["Authentication:Facebook:AppId"];
        var facebookAppSecret = configuration["Authentication:Facebook:AppSecret"];
        if (!string.IsNullOrWhiteSpace(facebookAppId) && !string.IsNullOrWhiteSpace(facebookAppSecret))
        {
            authBuilder.AddFacebook(options =>
            {
                options.AppId = facebookAppId;
                options.AppSecret = facebookAppSecret;
            });
        }

        // Twitter/X OAuth 2.0 (conditional)
        var twitterClientId = configuration["Authentication:Twitter:ClientId"];
        var twitterClientSecret = configuration["Authentication:Twitter:ClientSecret"];
        if (!string.IsNullOrWhiteSpace(twitterClientId) && !string.IsNullOrWhiteSpace(twitterClientSecret))
        {
            authBuilder.AddOAuth("Twitter", options =>
            {
                options.ClientId = twitterClientId;
                options.ClientSecret = twitterClientSecret;
                options.AuthorizationEndpoint = "https://twitter.com/i/oauth2/authorize";
                options.TokenEndpoint = "https://api.twitter.com/2/oauth2/token";
                options.UserInformationEndpoint = "https://api.twitter.com/2/users/me";
                options.Scope.Add("tweet.read");
                options.Scope.Add("users.read");
            });
        }

        // Authorization policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAuthenticated", policy => policy.RequireAuthenticatedUser());
            options.AddPolicy("RequireAdmin", policy =>
                policy.RequireAuthenticatedUser().RequireRole("Admin"));
        });

        // Rate limiting - fixed window, 10 requests/minute per IP for auth endpoints
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("auth", limiterOptions =>
            {
                limiterOptions.PermitLimit = 10;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });
        });

        // Register auth services
        services.AddScoped<IJwtService, JwtService>();
        services.AddTransient<IEmailService, ConsoleEmailService>();
        services.AddSingleton<IRefreshTokenService, InMemoryRefreshTokenService>();

        return services;
    }
}
