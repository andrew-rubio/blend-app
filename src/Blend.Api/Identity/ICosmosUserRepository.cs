using Blend.Api.Domain;

namespace Blend.Api.Identity;

public interface ICosmosUserRepository
{
    Task<BlendUser?> FindByIdAsync(string userId, CancellationToken ct);
    Task<BlendUser?> FindByEmailAsync(string email, CancellationToken ct);
    Task<BlendUser?> FindByUserNameAsync(string userName, CancellationToken ct);
    Task<BlendUser?> FindByLoginAsync(string provider, string providerKey, CancellationToken ct);
    Task CreateAsync(BlendUser user, CancellationToken ct);
    Task UpdateAsync(BlendUser user, CancellationToken ct);
    Task DeleteAsync(string userId, CancellationToken ct);
}
