using Blend.Domain.Identity;

namespace Blend.Api.Auth.Services;

public interface IJwtService
{
    string GenerateAccessToken(BlendUser user);
    string GenerateRefreshToken();
}
