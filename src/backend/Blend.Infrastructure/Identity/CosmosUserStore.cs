using Blend.Domain.Entities;
using Blend.Domain.Identity;
using Blend.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using IdentityExternalLoginInfo = Blend.Domain.Identity.ExternalLoginInfo;

namespace Blend.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity user store backed by Cosmos DB via <see cref="IRepository{BlendUser}"/>.
/// Implements password, email, external login, and role sub-stores.
/// </summary>
public sealed class CosmosUserStore :
    IUserStore<BlendUser>,
    IUserPasswordStore<BlendUser>,
    IUserEmailStore<BlendUser>,
    IUserLoginStore<BlendUser>,
    IUserRoleStore<BlendUser>
{
    private readonly IRepository<BlendUser> _users;

    public CosmosUserStore(IRepository<BlendUser> users)
    {
        _users = users;
    }

    public void Dispose() { }

    // ── IUserStore ───────────────────────────────────────────────────────────

    public async Task<IdentityResult> CreateAsync(BlendUser user, CancellationToken cancellationToken)
    {
        await _users.CreateAsync(user, cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(BlendUser user, CancellationToken cancellationToken)
    {
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _users.UpdateAsync(user, user.Id, user.Id, cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(BlendUser user, CancellationToken cancellationToken)
    {
        await _users.DeleteAsync(user.Id, user.Id, cancellationToken);
        return IdentityResult.Success;
    }

    public Task<string> GetUserIdAsync(BlendUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.Id);

    public Task<string?> GetUserNameAsync(BlendUser user, CancellationToken cancellationToken)
        => Task.FromResult<string?>(user.UserName);

    public Task SetUserNameAsync(BlendUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(BlendUser user, CancellationToken cancellationToken)
        => Task.FromResult<string?>(user.NormalizedUserName);

    public Task SetNormalizedUserNameAsync(BlendUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName ?? string.Empty;
        return Task.CompletedTask;
    }

    public async Task<BlendUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        => await _users.GetByIdAsync(userId, userId, cancellationToken);

    public async Task<BlendUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        var query = "SELECT * FROM c WHERE c.normalizedUserName = @normalizedUserName";
        var parameters = new Dictionary<string, object> { ["@normalizedUserName"] = normalizedUserName };
        var results = await _users.GetByQueryAsync(query, parameters, cancellationToken: cancellationToken);
        return results.FirstOrDefault();
    }

    // ── IUserPasswordStore ───────────────────────────────────────────────────

    public Task SetPasswordHashAsync(BlendUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(BlendUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.PasswordHash);

    public Task<bool> HasPasswordAsync(BlendUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.PasswordHash is not null);

    // ── IUserEmailStore ──────────────────────────────────────────────────────

    public Task SetEmailAsync(BlendUser user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email ?? string.Empty;
        // Identity uses Email as the unique UserName for this app; keep them in sync.
        user.UserName = email ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(BlendUser user, CancellationToken cancellationToken)
        => Task.FromResult<string?>(user.Email);

    public Task<bool> GetEmailConfirmedAsync(BlendUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.EmailConfirmed);

    public Task SetEmailConfirmedAsync(BlendUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public async Task<BlendUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        var query = "SELECT * FROM c WHERE c.normalizedEmail = @normalizedEmail";
        var parameters = new Dictionary<string, object> { ["@normalizedEmail"] = normalizedEmail };
        var results = await _users.GetByQueryAsync(query, parameters, cancellationToken: cancellationToken);
        return results.FirstOrDefault();
    }

    public Task<string?> GetNormalizedEmailAsync(BlendUser user, CancellationToken cancellationToken)
        => Task.FromResult<string?>(user.NormalizedEmail);

    public Task SetNormalizedEmailAsync(BlendUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail ?? string.Empty;
        return Task.CompletedTask;
    }

    // ── IUserLoginStore ──────────────────────────────────────────────────────

    public Task AddLoginAsync(BlendUser user, UserLoginInfo login, CancellationToken cancellationToken)
    {
        var alreadyLinked = user.ExternalLogins.Exists(
            l => l.LoginProvider == login.LoginProvider && l.ProviderKey == login.ProviderKey);

        if (!alreadyLinked)
        {
            user.ExternalLogins.Add(new IdentityExternalLoginInfo
            {
                LoginProvider = login.LoginProvider,
                ProviderKey = login.ProviderKey,
                ProviderDisplayName = login.ProviderDisplayName,
            });
        }

        return Task.CompletedTask;
    }

    public Task RemoveLoginAsync(BlendUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        user.ExternalLogins.RemoveAll(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
        return Task.CompletedTask;
    }

    public Task<IList<UserLoginInfo>> GetLoginsAsync(BlendUser user, CancellationToken cancellationToken)
    {
        IList<UserLoginInfo> logins = user.ExternalLogins
            .Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName))
            .ToList();
        return Task.FromResult(logins);
    }

    public async Task<BlendUser?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        // Cosmos SQL: find the first user whose externalLogins array contains a matching entry.
        var query =
            "SELECT * FROM c WHERE EXISTS(" +
            "SELECT VALUE l FROM l IN c.externalLogins " +
            "WHERE l.loginProvider = @loginProvider " +
            "AND l.providerKey = @providerKey)";

        var parameters = new Dictionary<string, object>
        {
            ["@loginProvider"] = loginProvider,
            ["@providerKey"] = providerKey,
        };

        var results = await _users.GetByQueryAsync(query, parameters, cancellationToken: cancellationToken);
        return results.FirstOrDefault();
    }

    // ── IUserRoleStore ───────────────────────────────────────────────────────

    public Task AddToRoleAsync(BlendUser user, string roleName, CancellationToken cancellationToken)
    {
        if (Enum.TryParse<UserRole>(roleName, ignoreCase: true, out var role))
        {
            user.Role = role;
        }
        return Task.CompletedTask;
    }

    public Task RemoveFromRoleAsync(BlendUser user, string roleName, CancellationToken cancellationToken)
    {
        if (Enum.TryParse<UserRole>(roleName, ignoreCase: true, out var role) && user.Role == role)
        {
            user.Role = UserRole.User;
        }
        return Task.CompletedTask;
    }

    public Task<IList<string>> GetRolesAsync(BlendUser user, CancellationToken cancellationToken)
    {
        IList<string> roles = [user.Role.ToString()];
        return Task.FromResult(roles);
    }

    public Task<bool> IsInRoleAsync(BlendUser user, string roleName, CancellationToken cancellationToken)
    {
        var isInRole = string.Equals(user.Role.ToString(), roleName, StringComparison.OrdinalIgnoreCase);
        return Task.FromResult(isInRole);
    }

    public async Task<IList<BlendUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        // Normalise to the enum value's name so the query matches what JsonStringEnumConverter stores.
        var storedRoleName = Enum.TryParse<UserRole>(roleName, ignoreCase: true, out var role)
            ? role.ToString()
            : roleName;

        var query = "SELECT * FROM c WHERE c.role = @roleName";
        var parameters = new Dictionary<string, object> { ["@roleName"] = storedRoleName };
        var results = await _users.GetByQueryAsync(query, parameters, cancellationToken: cancellationToken);
        return results.ToList();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
}
