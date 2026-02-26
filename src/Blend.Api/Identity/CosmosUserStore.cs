using Blend.Api.Domain;
using Microsoft.AspNetCore.Identity;

namespace Blend.Api.Identity;

public class CosmosUserStore :
    IUserStore<BlendUser>,
    IUserPasswordStore<BlendUser>,
    IUserEmailStore<BlendUser>,
    IUserRoleStore<BlendUser>,
    IUserLoginStore<BlendUser>
{
    private readonly ICosmosUserRepository _repo;

    public CosmosUserStore(ICosmosUserRepository repo) => _repo = repo;

    public async Task<IdentityResult> CreateAsync(BlendUser user, CancellationToken ct)
    {
        await _repo.CreateAsync(user, ct);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(BlendUser user, CancellationToken ct)
    {
        await _repo.DeleteAsync(user.Id, ct);
        return IdentityResult.Success;
    }

    public async Task<BlendUser?> FindByIdAsync(string userId, CancellationToken ct) =>
        await _repo.FindByIdAsync(userId, ct);

    public async Task<BlendUser?> FindByNameAsync(string normalizedUserName, CancellationToken ct) =>
        await _repo.FindByUserNameAsync(normalizedUserName, ct);

    public Task<string?> GetNormalizedUserNameAsync(BlendUser user, CancellationToken ct) =>
        Task.FromResult(user.NormalizedUserName);

    public Task<string> GetUserIdAsync(BlendUser user, CancellationToken ct) =>
        Task.FromResult(user.Id);

    public Task<string?> GetUserNameAsync(BlendUser user, CancellationToken ct) =>
        Task.FromResult(user.UserName);

    public Task SetNormalizedUserNameAsync(BlendUser user, string? normalizedName, CancellationToken ct)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(BlendUser user, string? userName, CancellationToken ct)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> UpdateAsync(BlendUser user, CancellationToken ct)
    {
        await _repo.UpdateAsync(user, ct);
        return IdentityResult.Success;
    }

    public Task<string?> GetPasswordHashAsync(BlendUser user, CancellationToken ct) =>
        Task.FromResult(user.PasswordHash);

    public Task<bool> HasPasswordAsync(BlendUser user, CancellationToken ct) =>
        Task.FromResult(user.PasswordHash != null);

    public Task SetPasswordHashAsync(BlendUser user, string? passwordHash, CancellationToken ct)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public async Task<BlendUser?> FindByEmailAsync(string normalizedEmail, CancellationToken ct) =>
        await _repo.FindByEmailAsync(normalizedEmail, ct);

    public Task<string?> GetEmailAsync(BlendUser user, CancellationToken ct) =>
        Task.FromResult(user.Email);

    public Task<bool> GetEmailConfirmedAsync(BlendUser user, CancellationToken ct) =>
        Task.FromResult(user.EmailConfirmed);

    public Task<string?> GetNormalizedEmailAsync(BlendUser user, CancellationToken ct) =>
        Task.FromResult(user.NormalizedEmail);

    public Task SetEmailAsync(BlendUser user, string? email, CancellationToken ct)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task SetEmailConfirmedAsync(BlendUser user, bool confirmed, CancellationToken ct)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task SetNormalizedEmailAsync(BlendUser user, string? normalizedEmail, CancellationToken ct)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    public Task AddToRoleAsync(BlendUser user, string roleName, CancellationToken ct)
    {
        if (!user.Roles.Contains(roleName, StringComparer.OrdinalIgnoreCase))
            user.Roles.Add(roleName);
        return Task.CompletedTask;
    }

    public Task<IList<string>> GetRolesAsync(BlendUser user, CancellationToken ct) =>
        Task.FromResult<IList<string>>(user.Roles);

    public Task<IList<BlendUser>> GetUsersInRoleAsync(string roleName, CancellationToken ct) =>
        Task.FromResult<IList<BlendUser>>(new List<BlendUser>());

    public Task<bool> IsInRoleAsync(BlendUser user, string roleName, CancellationToken ct) =>
        Task.FromResult(user.Roles.Contains(roleName, StringComparer.OrdinalIgnoreCase));

    public Task RemoveFromRoleAsync(BlendUser user, string roleName, CancellationToken ct)
    {
        user.Roles.RemoveAll(r => string.Equals(r, roleName, StringComparison.OrdinalIgnoreCase));
        return Task.CompletedTask;
    }

    public Task AddLoginAsync(BlendUser user, UserLoginInfo login, CancellationToken ct)
    {
        if (!user.ExternalLogins.Any(l => l.Provider == login.LoginProvider && l.ProviderKey == login.ProviderKey))
        {
            user.ExternalLogins.Add(new ExternalLogin
            {
                Provider = login.LoginProvider,
                ProviderKey = login.ProviderKey,
                DisplayName = login.ProviderDisplayName ?? login.LoginProvider
            });
        }
        return Task.CompletedTask;
    }

    public async Task<BlendUser?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken ct) =>
        await _repo.FindByLoginAsync(loginProvider, providerKey, ct);

    public Task<IList<UserLoginInfo>> GetLoginsAsync(BlendUser user, CancellationToken ct) =>
        Task.FromResult<IList<UserLoginInfo>>(
            user.ExternalLogins.Select(l => new UserLoginInfo(l.Provider, l.ProviderKey, l.DisplayName)).ToList());

    public Task RemoveLoginAsync(BlendUser user, string loginProvider, string providerKey, CancellationToken ct)
    {
        user.ExternalLogins.RemoveAll(l => l.Provider == loginProvider && l.ProviderKey == providerKey);
        return Task.CompletedTask;
    }

    public void Dispose() { }
}
