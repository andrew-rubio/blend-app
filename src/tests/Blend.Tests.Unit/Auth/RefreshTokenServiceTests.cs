using Blend.Api.Auth;
using Blend.Api.Auth.Services;
using Microsoft.Extensions.Options;

namespace Blend.Tests.Unit.Auth;

public class RefreshTokenServiceTests
{
    private static InMemoryRefreshTokenService CreateService(int expiryDays = 7)
    {
        var options = Options.Create(new JwtOptions { RefreshTokenExpiryDays = expiryDays });
        return new InMemoryRefreshTokenService(options);
    }

    [Fact]
    public async Task CreateAsync_StoresToken()
    {
        var service = CreateService();

        var token = await service.CreateAsync("user-1", "token-value");

        Assert.NotNull(token);
        Assert.Equal("user-1", token.UserId);
        Assert.Equal("token-value", token.Token);
        Assert.False(token.IsRevoked);
    }

    [Fact]
    public async Task GetValidAsync_WithValidToken_ReturnsToken()
    {
        var service = CreateService();
        await service.CreateAsync("user-1", "valid-token");

        var result = await service.GetValidAsync("valid-token");

        Assert.NotNull(result);
        Assert.Equal("valid-token", result.Token);
    }

    [Fact]
    public async Task GetValidAsync_WithRevokedToken_ReturnsNull()
    {
        var service = CreateService();
        await service.CreateAsync("user-1", "token-to-revoke");
        await service.RevokeAsync("token-to-revoke");

        var result = await service.GetValidAsync("token-to-revoke");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetValidAsync_WithExpiredToken_ReturnsNull()
    {
        // Use a negative expiry so the token is immediately past its expiry date
        var service = CreateService(expiryDays: -1);
        await service.CreateAsync("user-1", "expired-token");

        var result = await service.GetValidAsync("expired-token");

        Assert.Null(result);
    }

    [Fact]
    public async Task RevokeAsync_MarksTokenRevoked()
    {
        var service = CreateService();
        var token = await service.CreateAsync("user-1", "token-to-revoke");

        await service.RevokeAsync("token-to-revoke");

        Assert.True(token.IsRevoked);
    }

    [Fact]
    public async Task CreateAsync_RefreshToken_HasCorrectExpiry()
    {
        var service = CreateService(expiryDays: 7);
        var minExpectedExpiry = DateTimeOffset.UtcNow.AddDays(6);
        var maxExpectedExpiry = DateTimeOffset.UtcNow.AddDays(8);

        var token = await service.CreateAsync("user-1", "token-value");

        Assert.True(token.ExpiresAt >= minExpectedExpiry && token.ExpiresAt <= maxExpectedExpiry);
    }

    [Fact]
    public async Task GetValidAsync_WithNonExistentToken_ReturnsNull()
    {
        var service = CreateService();

        var result = await service.GetValidAsync("nonexistent-token");

        Assert.Null(result);
    }
}
