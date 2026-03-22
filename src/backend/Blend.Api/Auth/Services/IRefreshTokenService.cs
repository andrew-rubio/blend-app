using Blend.Domain.Identity;

namespace Blend.Api.Auth.Services;

public interface IRefreshTokenService
{
    Task<RefreshToken> CreateAsync(string userId, string token, CancellationToken ct = default);
    Task<RefreshToken?> GetValidAsync(string token, CancellationToken ct = default);
    Task RevokeAsync(string token, CancellationToken ct = default);
}
