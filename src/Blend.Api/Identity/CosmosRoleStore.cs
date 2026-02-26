using Blend.Api.Domain;
using Microsoft.AspNetCore.Identity;

namespace Blend.Api.Identity;

public class CosmosRoleStore : IRoleStore<BlendRole>
{
    private readonly List<BlendRole> _roles = new()
    {
        new BlendRole { Id = "1", Name = "Admin" },
        new BlendRole { Id = "2", Name = "User" }
    };

    public Task<IdentityResult> CreateAsync(BlendRole role, CancellationToken ct)
    {
        _roles.Add(role);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(BlendRole role, CancellationToken ct)
    {
        _roles.RemoveAll(r => r.Id == role.Id);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<BlendRole?> FindByIdAsync(string roleId, CancellationToken ct) =>
        Task.FromResult(_roles.FirstOrDefault(r => r.Id == roleId));

    public Task<BlendRole?> FindByNameAsync(string normalizedRoleName, CancellationToken ct) =>
        Task.FromResult(_roles.FirstOrDefault(r => string.Equals(r.Name, normalizedRoleName, StringComparison.OrdinalIgnoreCase)));

    public Task<string?> GetNormalizedRoleNameAsync(BlendRole role, CancellationToken ct) =>
        Task.FromResult<string?>(role.Name.ToUpperInvariant());

    public Task<string> GetRoleIdAsync(BlendRole role, CancellationToken ct) =>
        Task.FromResult(role.Id);

    public Task<string?> GetRoleNameAsync(BlendRole role, CancellationToken ct) =>
        Task.FromResult<string?>(role.Name);

    public Task SetNormalizedRoleNameAsync(BlendRole role, string? normalizedName, CancellationToken ct) =>
        Task.CompletedTask;

    public Task SetRoleNameAsync(BlendRole role, string? roleName, CancellationToken ct)
    {
        role.Name = roleName ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<IdentityResult> UpdateAsync(BlendRole role, CancellationToken ct)
    {
        var idx = _roles.FindIndex(r => r.Id == role.Id);
        if (idx >= 0) _roles[idx] = role;
        return Task.FromResult(IdentityResult.Success);
    }

    public void Dispose() { }
}
