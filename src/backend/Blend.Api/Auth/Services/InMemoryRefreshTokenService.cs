using System.Collections.Concurrent;
using Blend.Domain.Identity;
using Microsoft.Extensions.Options;

namespace Blend.Api.Auth.Services;

public sealed class InMemoryRefreshTokenService : IRefreshTokenService
{
    private readonly ConcurrentDictionary<string, RefreshToken> _store = new();
    private readonly JwtOptions _options;

    public InMemoryRefreshTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public Task<RefreshToken> CreateAsync(string userId, string token, CancellationToken ct = default)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenExpiryDays),
            IsRevoked = false,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _store[token] = refreshToken;
        return Task.FromResult(refreshToken);
    }

    public Task<RefreshToken?> GetValidAsync(string token, CancellationToken ct = default)
    {
        _store.TryGetValue(token, out var refreshToken);
        if (refreshToken is null || refreshToken.IsRevoked || refreshToken.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return Task.FromResult<RefreshToken?>(null);
        }
        return Task.FromResult<RefreshToken?>(refreshToken);
    }

    public Task RevokeAsync(string token, CancellationToken ct = default)
    {
        if (_store.TryGetValue(token, out var refreshToken))
        {
            refreshToken.IsRevoked = true;
        }
        return Task.CompletedTask;
    }
}
