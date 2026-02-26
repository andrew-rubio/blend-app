using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blend.Api.Configuration;
using Blend.Api.Domain;
using Blend.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Blend.Api.Tests;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _service;
    private readonly JwtSettings _settings = new()
    {
        Issuer = "test-issuer",
        Audience = "test-audience",
        SecretKey = "test-secret-key-that-is-at-least-32-chars-long!",
        AccessTokenExpirationMinutes = 20,
        RefreshTokenExpirationDays = 7
    };

    public JwtTokenServiceTests() =>
        _service = new JwtTokenService(Options.Create(_settings), Microsoft.Extensions.Logging.Abstractions.NullLogger<JwtTokenService>.Instance);

    [Fact]
    public void GenerateAccessToken_ReturnsValidJwt()
    {
        var user = new BlendUser { Id = "u1", Email = "test@test.com", DisplayName = "Test" };
        var token = _service.GenerateAccessToken(user, new[] { "User" });
        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public void GenerateAccessToken_ContainsExpectedClaims()
    {
        var user = new BlendUser { Id = "u1", Email = "test@test.com", DisplayName = "Test User" };
        var token = _service.GenerateAccessToken(user, new[] { "User", "Admin" });
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        jwt.Subject.Should().Be("u1");
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "test@test.com");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateAccessToken_HasCorrectExpiry()
    {
        var user = new BlendUser { Id = "u1", Email = "e@e.com", DisplayName = "E" };
        var token = _service.GenerateAccessToken(user, Array.Empty<string>());
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var expectedExpiry = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes);
        jwt.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ValidateToken_ReturnsClaimsPrincipal_ForValidToken()
    {
        var user = new BlendUser { Id = "u1", Email = "e@e.com", DisplayName = "E" };
        var token = _service.GenerateAccessToken(user, new[] { "User" });
        var principal = _service.ValidateToken(token);
        principal.Should().NotBeNull();
        // JwtSecurityTokenHandler maps "sub" -> ClaimTypes.NameIdentifier by default
        principal!.FindFirstValue(ClaimTypes.NameIdentifier).Should().Be("u1");
    }

    [Fact]
    public void ValidateToken_ReturnsNull_ForInvalidToken()
    {
        var principal = _service.ValidateToken("invalid.token.value");
        principal.Should().BeNull();
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueTokens()
    {
        var (t1, _) = _service.GenerateRefreshToken();
        var (t2, _) = _service.GenerateRefreshToken();
        t1.Should().NotBe(t2);
    }

    [Fact]
    public void GenerateRefreshToken_ExpiresAfterConfiguredDays()
    {
        var (_, expiresAt) = _service.GenerateRefreshToken();
        var expected = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);
        expiresAt.Should().BeCloseTo(expected, TimeSpan.FromSeconds(5));
    }
}
