using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Blend.Api.Auth.Models;
using Blend.Domain.Entities;
using Blend.Domain.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blend.Tests.Integration;

// ── In-memory user store that replaces CosmosUserStore in tests ───────────────

internal sealed class InMemoryUserStore :
    IUserStore<BlendUser>,
    IUserPasswordStore<BlendUser>,
    IUserEmailStore<BlendUser>,
    IUserLoginStore<BlendUser>,
    IUserRoleStore<BlendUser>,
    IUserSecurityStampStore<BlendUser>
{
    private readonly ConcurrentDictionary<string, BlendUser> _users = new();

    public void Dispose() { }

    // IUserStore

    public Task<IdentityResult> CreateAsync(BlendUser user, CancellationToken ct)
    {
        _users[user.Id] = user;
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> UpdateAsync(BlendUser user, CancellationToken ct)
    {
        _users[user.Id] = user;
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(BlendUser user, CancellationToken ct)
    {
        _users.TryRemove(user.Id, out _);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<BlendUser?> FindByIdAsync(string userId, CancellationToken ct)
        => Task.FromResult(_users.GetValueOrDefault(userId));

    public Task<BlendUser?> FindByNameAsync(string normalizedUserName, CancellationToken ct)
        => Task.FromResult(
            _users.Values.FirstOrDefault(u => u.NormalizedUserName == normalizedUserName));

    public Task<string> GetUserIdAsync(BlendUser user, CancellationToken ct)
        => Task.FromResult(user.Id);

    public Task<string?> GetUserNameAsync(BlendUser user, CancellationToken ct)
        => Task.FromResult<string?>(user.UserName);

    public Task SetUserNameAsync(BlendUser user, string? userName, CancellationToken ct)
    {
        user.UserName = userName ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(BlendUser user, CancellationToken ct)
        => Task.FromResult<string?>(user.NormalizedUserName);

    public Task SetNormalizedUserNameAsync(BlendUser user, string? normalizedName, CancellationToken ct)
    {
        user.NormalizedUserName = normalizedName ?? string.Empty;
        return Task.CompletedTask;
    }

    // IUserPasswordStore

    public Task SetPasswordHashAsync(BlendUser user, string? passwordHash, CancellationToken ct)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(BlendUser user, CancellationToken ct)
        => Task.FromResult(user.PasswordHash);

    public Task<bool> HasPasswordAsync(BlendUser user, CancellationToken ct)
        => Task.FromResult(user.PasswordHash is not null);

    // IUserEmailStore

    public Task SetEmailAsync(BlendUser user, string? email, CancellationToken ct)
    {
        user.Email = email ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(BlendUser user, CancellationToken ct)
        => Task.FromResult<string?>(user.Email);

    public Task<bool> GetEmailConfirmedAsync(BlendUser user, CancellationToken ct)
        => Task.FromResult(user.EmailConfirmed);

    public Task SetEmailConfirmedAsync(BlendUser user, bool confirmed, CancellationToken ct)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task<BlendUser?> FindByEmailAsync(string normalizedEmail, CancellationToken ct)
        => Task.FromResult(
            _users.Values.FirstOrDefault(u => u.NormalizedEmail == normalizedEmail));

    public Task<string?> GetNormalizedEmailAsync(BlendUser user, CancellationToken ct)
        => Task.FromResult<string?>(user.NormalizedEmail);

    public Task SetNormalizedEmailAsync(BlendUser user, string? normalizedEmail, CancellationToken ct)
    {
        user.NormalizedEmail = normalizedEmail ?? string.Empty;
        return Task.CompletedTask;
    }

    // IUserLoginStore

    public Task AddLoginAsync(BlendUser user, UserLoginInfo login, CancellationToken ct)
        => Task.CompletedTask;

    public Task RemoveLoginAsync(BlendUser user, string loginProvider, string providerKey, CancellationToken ct)
        => Task.CompletedTask;

    public Task<IList<UserLoginInfo>> GetLoginsAsync(BlendUser user, CancellationToken ct)
        => Task.FromResult<IList<UserLoginInfo>>(new List<UserLoginInfo>());

    public Task<BlendUser?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken ct)
        => Task.FromResult<BlendUser?>(null);

    // IUserRoleStore

    public Task AddToRoleAsync(BlendUser user, string roleName, CancellationToken ct)
        => Task.CompletedTask;

    public Task RemoveFromRoleAsync(BlendUser user, string roleName, CancellationToken ct)
        => Task.CompletedTask;

    public Task<IList<string>> GetRolesAsync(BlendUser user, CancellationToken ct)
        => Task.FromResult<IList<string>>(new List<string> { user.Role.ToString() });

    public Task<bool> IsInRoleAsync(BlendUser user, string roleName, CancellationToken ct)
        => Task.FromResult(
            user.Role.ToString().Equals(roleName, StringComparison.OrdinalIgnoreCase));

    public Task<IList<BlendUser>> GetUsersInRoleAsync(string roleName, CancellationToken ct)
        => Task.FromResult<IList<BlendUser>>(
            _users.Values
                .Where(u => u.Role.ToString().Equals(roleName, StringComparison.OrdinalIgnoreCase))
                .ToList());

    // IUserSecurityStampStore

    public Task SetSecurityStampAsync(BlendUser user, string stamp, CancellationToken ct)
    {
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(BlendUser user, CancellationToken ct)
        => Task.FromResult<string?>(user.SecurityStamp);
}

// ── Test factory ──────────────────────────────────────────────────────────────

public sealed class AuthTestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set a valid JWT secret for signing/validation during tests only — do NOT use in production
        builder.UseSetting("Jwt:SecretKey",
            "test-secret-key-that-is-long-enough-for-hs256-algorithm");
        builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Development");

        builder.ConfigureServices(services =>
        {
            // Replace CosmosUserStore with the in-memory implementation
            services.RemoveAll<IUserStore<BlendUser>>();
            services.AddSingleton<IUserStore<BlendUser>, InMemoryUserStore>();
        });
    }
}

// ── Integration tests ─────────────────────────────────────────────────────────

public class AuthFlowTests : IClassFixture<AuthTestWebApplicationFactory>
{
    private readonly AuthTestWebApplicationFactory _factory;

    public AuthFlowTests(AuthTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false,
        });

    // Each test uses a unique email to stay independent within the shared factory.
    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";

    private static StringContent JsonBody(object payload) =>
        new(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

    private static string? ExtractCookie(HttpResponseMessage response, string name)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
            return null;

        foreach (var cookie in cookies)
        {
            if (cookie.StartsWith($"{name}=", StringComparison.OrdinalIgnoreCase))
                return cookie.Split(';')[0][(name.Length + 1)..];
        }

        return null;
    }

    [Fact]
    public async Task Register_WithValidData_Returns201WithAccessToken()
    {
        var client = CreateClient();
        var body = JsonBody(new { displayName = "Alice", email = UniqueEmail(), password = "ValidPass1!" });

        var response = await client.PostAsync("/api/v1/auth/register", body);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("accessToken", content);
    }

    [Fact]
    public async Task Register_WithExistingEmail_Returns409()
    {
        var client = CreateClient();
        var email = UniqueEmail();
        var body = JsonBody(new { displayName = "Bob", email, password = "ValidPass1!" });

        await client.PostAsync("/api/v1/auth/register", body);
        var secondResponse = await client.PostAsync("/api/v1/auth/register",
            JsonBody(new { displayName = "Bob2", email, password = "ValidPass1!" }));

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithAccessToken()
    {
        var client = CreateClient();
        var email = UniqueEmail();
        await client.PostAsync("/api/v1/auth/register",
            JsonBody(new { displayName = "Carol", email, password = "ValidPass1!" }));

        var response = await client.PostAsync("/api/v1/auth/login",
            JsonBody(new { email, password = "ValidPass1!" }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("accessToken", content);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401()
    {
        var client = CreateClient();
        var email = UniqueEmail();
        await client.PostAsync("/api/v1/auth/register",
            JsonBody(new { displayName = "Dave", email, password = "ValidPass1!" }));

        var response = await client.PostAsync("/api/v1/auth/login",
            JsonBody(new { email, password = "WrongPass1!" }));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_Returns401()
    {
        var client = CreateClient();

        var response = await client.PostAsync("/api/v1/auth/login",
            JsonBody(new { email = UniqueEmail(), password = "ValidPass1!" }));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithValidCookie_Returns200WithNewToken()
    {
        var client = CreateClient();
        var registerResponse = await client.PostAsync("/api/v1/auth/register",
            JsonBody(new { displayName = "Eve", email = UniqueEmail(), password = "ValidPass1!" }));

        var refreshToken = ExtractCookie(registerResponse, "refresh_token");
        Assert.NotNull(refreshToken);

        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
        refreshRequest.Headers.Add("Cookie", $"refresh_token={refreshToken}");

        var refreshResponse = await client.SendAsync(refreshRequest);

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var content = await refreshResponse.Content.ReadAsStringAsync();
        Assert.Contains("accessToken", content);
    }

    [Fact]
    public async Task Logout_Returns200()
    {
        var client = CreateClient();
        var registerResponse = await client.PostAsync("/api/v1/auth/register",
            JsonBody(new { displayName = "Frank", email = UniqueEmail(), password = "ValidPass1!" }));

        var refreshToken = ExtractCookie(registerResponse, "refresh_token");

        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        if (refreshToken is not null)
            logoutRequest.Headers.Add("Cookie", $"refresh_token={refreshToken}");

        var logoutResponse = await client.SendAsync(logoutRequest);

        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_AlwaysReturns200()
    {
        var client = CreateClient();

        var response = await client.PostAsync("/api/v1/auth/forgot-password",
            JsonBody(new { email = UniqueEmail() }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("message", content);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_Returns400()
    {
        var client = CreateClient();
        var email = UniqueEmail();
        await client.PostAsync("/api/v1/auth/register",
            JsonBody(new { displayName = "Grace", email, password = "ValidPass1!" }));

        var response = await client.PostAsync("/api/v1/auth/reset-password",
            JsonBody(new { email, token = "invalid-token", newPassword = "NewPass1!" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
