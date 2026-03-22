using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blend.Api.Auth;
using Blend.Api.Auth.Services;
using Blend.Domain.Entities;
using Blend.Domain.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Blend.Tests.Unit.Auth;

public class JwtServiceTests
{
    private static readonly JwtOptions TestOptions = new()
    {
        SecretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256",
        Issuer = "test-issuer",
        Audience = "test-audience",
        AccessTokenExpiryMinutes = 15,
        RefreshTokenExpiryDays = 7,
    };

    private static JwtService CreateService() => new(Options.Create(TestOptions));

    private static BlendUser CreateUser(UserRole role = UserRole.User) => new()
    {
        Id = "user-123",
        Email = "test@example.com",
        DisplayName = "Test User",
        Role = role,
    };

    [Fact]
    public void GenerateAccessToken_ReturnsValidJwt()
    {
        var service = CreateService();
        var user = CreateUser();

        var token = service.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        handler.InboundClaimTypeMap.Clear(); // preserve raw JWT claim names
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = TestOptions.Issuer,
            ValidAudience = TestOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(TestOptions.SecretKey)),
        };

        var principal = handler.ValidateToken(token, parameters, out _);

        Assert.Equal(user.Id, principal.FindFirstValue(JwtRegisteredClaimNames.Sub));
        Assert.Equal(user.Email, principal.FindFirstValue(JwtRegisteredClaimNames.Email));
        Assert.Equal(user.DisplayName, principal.FindFirstValue("name"));
        Assert.Equal(user.Role.ToString(), principal.FindFirstValue(ClaimTypes.Role));
    }

    [Fact]
    public void GenerateAccessToken_HasCorrectExpiry()
    {
        var service = CreateService();
        var user = CreateUser();

        var minExpectedExpiry = DateTimeOffset.UtcNow.AddMinutes(TestOptions.AccessTokenExpiryMinutes - 1);
        var maxExpectedExpiry = DateTimeOffset.UtcNow.AddMinutes(TestOptions.AccessTokenExpiryMinutes + 1);

        var token = service.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var expiry = new DateTimeOffset(jwtToken.ValidTo, TimeSpan.Zero);

        Assert.True(expiry >= minExpectedExpiry && expiry <= maxExpectedExpiry);
    }

    [Fact]
    public void GenerateAccessToken_HasCorrectIssuerAndAudience()
    {
        var service = CreateService();
        var user = CreateUser();

        var token = service.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal(TestOptions.Issuer, jwtToken.Issuer);
        Assert.Contains(TestOptions.Audience, jwtToken.Audiences);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        var service = CreateService();

        var token = service.GenerateRefreshToken();

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsDifferentValuesEachTime()
    {
        var service = CreateService();

        var token1 = service.GenerateRefreshToken();
        var token2 = service.GenerateRefreshToken();

        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GenerateAccessToken_WithAdminRole_IncludesAdminRole()
    {
        var service = CreateService();
        var user = CreateUser(UserRole.Admin);

        var token = service.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

        Assert.NotNull(roleClaim);
        Assert.Equal(UserRole.Admin.ToString(), roleClaim.Value);
    }
}
