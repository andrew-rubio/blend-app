using Blend.Domain.Entities;
using Blend.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace Blend.Infrastructure.Identity;

/// <summary>
/// In-memory ASP.NET Core Identity role store.
/// Roles are predefined (User, Admin) and do not require Cosmos DB persistence.
/// </summary>
public sealed class CosmosRoleStore : IRoleStore<BlendRole>
{
    private static readonly IReadOnlyDictionary<string, BlendRole> PredefinedRoles =
        new Dictionary<string, BlendRole>(StringComparer.OrdinalIgnoreCase)
        {
            [UserRole.User.ToString()] = new BlendRole
            {
                Id = "1",
                Name = UserRole.User.ToString(),
                NormalizedName = UserRole.User.ToString().ToUpperInvariant(),
                ConcurrencyStamp = "user-stamp",
            },
            [UserRole.Admin.ToString()] = new BlendRole
            {
                Id = "2",
                Name = UserRole.Admin.ToString(),
                NormalizedName = UserRole.Admin.ToString().ToUpperInvariant(),
                ConcurrencyStamp = "admin-stamp",
            },
        };

    public void Dispose() { }

    public Task<IdentityResult> CreateAsync(BlendRole role, CancellationToken cancellationToken)
        => Task.FromResult(IdentityResult.Success);

    public Task<IdentityResult> UpdateAsync(BlendRole role, CancellationToken cancellationToken)
        => Task.FromResult(IdentityResult.Success);

    public Task<IdentityResult> DeleteAsync(BlendRole role, CancellationToken cancellationToken)
        => Task.FromResult(IdentityResult.Success);

    public Task<string> GetRoleIdAsync(BlendRole role, CancellationToken cancellationToken)
        => Task.FromResult(role.Id);

    public Task<string?> GetRoleNameAsync(BlendRole role, CancellationToken cancellationToken)
        => Task.FromResult<string?>(role.Name);

    public Task SetRoleNameAsync(BlendRole role, string? roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedRoleNameAsync(BlendRole role, CancellationToken cancellationToken)
        => Task.FromResult<string?>(role.NormalizedName);

    public Task SetNormalizedRoleNameAsync(BlendRole role, string? normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<BlendRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        var role = PredefinedRoles.Values.FirstOrDefault(r => r.Id == roleId);
        return Task.FromResult(role);
    }

    public Task<BlendRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        PredefinedRoles.TryGetValue(normalizedRoleName, out var role);
        return Task.FromResult(role);
    }
}
