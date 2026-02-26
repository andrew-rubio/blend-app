using Blend.Api.Domain;

namespace Blend.Api.Identity;

public class InMemoryUserRepository : ICosmosUserRepository
{
    private readonly List<BlendUser> _users = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<BlendUser?> FindByIdAsync(string userId, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try { return _users.FirstOrDefault(u => u.Id == userId); }
        finally { _lock.Release(); }
    }

    public async Task<BlendUser?> FindByEmailAsync(string email, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try { return _users.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)); }
        finally { _lock.Release(); }
    }

    public async Task<BlendUser?> FindByUserNameAsync(string userName, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try { return _users.FirstOrDefault(u => string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase)); }
        finally { _lock.Release(); }
    }

    public async Task<BlendUser?> FindByLoginAsync(string provider, string providerKey, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try { return _users.FirstOrDefault(u => u.ExternalLogins.Any(l => l.Provider == provider && l.ProviderKey == providerKey)); }
        finally { _lock.Release(); }
    }

    public async Task CreateAsync(BlendUser user, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try { _users.Add(user); }
        finally { _lock.Release(); }
    }

    public async Task UpdateAsync(BlendUser user, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var idx = _users.FindIndex(u => u.Id == user.Id);
            if (idx >= 0) _users[idx] = user;
        }
        finally { _lock.Release(); }
    }

    public async Task DeleteAsync(string userId, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try { _users.RemoveAll(u => u.Id == userId); }
        finally { _lock.Release(); }
    }
}
