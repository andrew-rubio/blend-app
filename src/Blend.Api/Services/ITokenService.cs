using System.Security.Claims;
using Blend.Api.Domain;

namespace Blend.Api.Services;

public interface ITokenService
{
    string GenerateAccessToken(BlendUser user, IList<string> roles);
    (string refreshToken, DateTime expiresAt) GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}
