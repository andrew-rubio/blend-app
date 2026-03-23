using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Blend.Api.Auth.Models;
using Blend.Domain.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blend.Tests.Integration;

// ── Test Factory ──────────────────────────────────────────────────────────────

public sealed class ProfileTestFactory : WebApplicationFactory<Program>
{
    private readonly InMemoryUserStore _userStore = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:SecretKey", "test-secret-key-that-is-long-enough-for-hs256-algorithm");
        builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IUserStore<BlendUser>>();
            services.AddSingleton<IUserStore<BlendUser>>(_userStore);
        });
    }
}

// ── Integration Tests ─────────────────────────────────────────────────────────

public class ProfileEndpointTests : IClassFixture<ProfileTestFactory>
{
    private readonly ProfileTestFactory _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public ProfileEndpointTests(ProfileTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = false,
    });

    private static string UniqueEmail() => $"profile-{Guid.NewGuid():N}@example.com";

    private static StringContent JsonBody(object payload) =>
        new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    private async Task<(HttpClient Client, string UserId, string Email)> RegisterAndAuthenticateAsync(
        string displayName = "ProfileUser")
    {
        var client = CreateClient();
        var email = UniqueEmail();
        var registerResponse = await client.PostAsync("/api/v1/auth/register",
            JsonBody(new { displayName, email, password = "ValidPass1!" }));
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var authBody = await registerResponse.Content.ReadAsStringAsync();
        var auth = JsonSerializer.Deserialize<AuthResponse>(authBody, JsonOptions);
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var userId = ExtractUserIdFromJwt(auth.AccessToken);
        return (client, userId, email);
    }

    private static string ExtractUserIdFromJwt(string token)
    {
        var parts = token.Split('.');
        var payload = parts[1];
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("sub").GetString()!;
    }

    // 1. GET /api/v1/users/me/profile without auth → 401
    [Fact]
    public async Task GetMyProfile_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/users/me/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // 2. GET /api/v1/users/me/profile authenticated → 200 with correct data
    [Fact]
    public async Task GetMyProfile_Authenticated_Returns200WithProfile()
    {
        var (client, userId, email) = await RegisterAndAuthenticateAsync("TestUser");

        var response = await client.GetAsync("/api/v1/users/me/profile");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var profile = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);

        Assert.Equal(userId, profile.GetProperty("id").GetString());
        Assert.Equal("TestUser", profile.GetProperty("displayName").GetString());
        Assert.Equal(email, profile.GetProperty("email").GetString());
        Assert.True(profile.TryGetProperty("joinDate", out _));
        Assert.True(profile.TryGetProperty("recipeCount", out _));
        Assert.True(profile.TryGetProperty("followerCount", out _));
        Assert.True(profile.TryGetProperty("followingCount", out _));
    }

    // 3. PUT /api/v1/users/me/profile without auth → 401
    [Fact]
    public async Task UpdateMyProfile_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var response = await client.PutAsync("/api/v1/users/me/profile",
            JsonBody(new { displayName = "New Name", bio = (string?)null, avatarUrl = (string?)null }));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // 4. PUT /api/v1/users/me/profile with valid data → 200
    [Fact]
    public async Task UpdateMyProfile_WithValidData_Returns200()
    {
        var (client, _, email) = await RegisterAndAuthenticateAsync();

        var response = await client.PutAsync("/api/v1/users/me/profile",
            JsonBody(new { displayName = "UpdatedName", bio = "A short bio.", avatarUrl = (string?)null }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var profile = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);
        Assert.Equal("UpdatedName", profile.GetProperty("displayName").GetString());
        Assert.Equal("A short bio.", profile.GetProperty("bio").GetString());
    }

    // 5. PUT /api/v1/users/me/profile with empty display name → 400
    [Fact]
    public async Task UpdateMyProfile_EmptyDisplayName_Returns400()
    {
        var (client, _, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PutAsync("/api/v1/users/me/profile",
            JsonBody(new { displayName = "", bio = (string?)null, avatarUrl = (string?)null }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // 6. PUT /api/v1/users/me/profile with display name too short → 400
    [Fact]
    public async Task UpdateMyProfile_DisplayNameTooShort_Returns400()
    {
        var (client, _, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PutAsync("/api/v1/users/me/profile",
            JsonBody(new { displayName = "X", bio = (string?)null, avatarUrl = (string?)null }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // 7. PUT /api/v1/users/me/profile with display name too long → 400
    [Fact]
    public async Task UpdateMyProfile_DisplayNameTooLong_Returns400()
    {
        var (client, _, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PutAsync("/api/v1/users/me/profile",
            JsonBody(new { displayName = new string('A', 51), bio = (string?)null, avatarUrl = (string?)null }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // 8. PUT /api/v1/users/me/profile with bio too long → 400
    [Fact]
    public async Task UpdateMyProfile_BioTooLong_Returns400()
    {
        var (client, _, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PutAsync("/api/v1/users/me/profile",
            JsonBody(new { displayName = "ValidName", bio = new string('B', 501), avatarUrl = (string?)null }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // 9. PUT /api/v1/users/me/profile with display name containing special chars → 400
    [Fact]
    public async Task UpdateMyProfile_DisplayNameWithSpecialChars_Returns400()
    {
        var (client, _, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PutAsync("/api/v1/users/me/profile",
            JsonBody(new { displayName = "Name!@#", bio = (string?)null, avatarUrl = (string?)null }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // 10. GET /api/v1/users/{userId}/profile for existing user → 200 with public info only
    [Fact]
    public async Task GetPublicProfile_ExistingUser_Returns200WithPublicInfoOnly()
    {
        var (_, userId, _) = await RegisterAndAuthenticateAsync("PublicUser");

        var anonClient = CreateClient();
        var response = await anonClient.GetAsync($"/api/v1/users/{userId}/profile");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var profile = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);

        Assert.Equal(userId, profile.GetProperty("id").GetString());
        Assert.Equal("PublicUser", profile.GetProperty("displayName").GetString());
        Assert.True(profile.TryGetProperty("joinDate", out _));
        Assert.True(profile.TryGetProperty("recipeCount", out _));

        // Email must NOT be in the public profile response
        Assert.False(profile.TryGetProperty("email", out _));
        Assert.False(profile.TryGetProperty("followerCount", out _));
        Assert.False(profile.TryGetProperty("followingCount", out _));
    }

    // 11. GET /api/v1/users/{userId}/profile for non-existent user → 404
    [Fact]
    public async Task GetPublicProfile_NonExistentUser_Returns404()
    {
        var client = CreateClient();
        var response = await client.GetAsync($"/api/v1/users/{Guid.NewGuid()}/profile");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // 12. PUT /api/v1/users/me/profile with avatar URL → 200 (avatar URL is updated)
    [Fact]
    public async Task UpdateMyProfile_WithAvatarUrl_Returns200WithAvatarUrl()
    {
        var (client, _, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PutAsync("/api/v1/users/me/profile",
            JsonBody(new { displayName = "AvatarUser", bio = (string?)null, avatarUrl = "https://example.com/avatar.jpg" }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var profile = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);
        Assert.Equal("https://example.com/avatar.jpg", profile.GetProperty("avatarUrl").GetString());
    }
}
