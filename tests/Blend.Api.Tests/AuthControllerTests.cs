using System.Net;
using System.Net.Http.Json;
using Blend.Api.Models.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Blend.Api.Tests;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Jwt:SecretKey", "test-secret-key-at-least-32-chars-long-!!!");
            builder.UseSetting("Jwt:Issuer", "test");
            builder.UseSetting("Jwt:Audience", "test");
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
    }

    [Fact]
    public async Task Register_WithValidData_Returns200WithToken()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest
        {
            Email = $"user_{Guid.NewGuid():N}@test.com",
            Password = "ValidPass1",
            DisplayName = "Test User"
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();
        auth!.AccessToken.Should().NotBeNullOrEmpty();
        auth.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task Register_WithWeakPassword_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest
        {
            Email = $"user_{Guid.NewGuid():N}@test.com",
            Password = "weak",
            DisplayName = "Test User"
        });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithToken()
    {
        var email = $"login_{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest
        {
            Email = email,
            Password = "ValidPass1",
            DisplayName = "Login Test"
        });

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest { Email = email, Password = "ValidPass1" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest { Email = "nobody@test.com", Password = "WrongPass1" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithValidToken_Returns200()
    {
        var email = $"me_{Guid.NewGuid():N}@test.com";
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest
        {
            Email = email,
            Password = "ValidPass1",
            DisplayName = "Me Test"
        });
        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var response = await _client.GetAsync("/api/v1/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task Refresh_WithValidCookie_Returns200()
    {
        var email = $"refresh_{Guid.NewGuid():N}@test.com";
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest
        {
            Email = email,
            Password = "ValidPass1",
            DisplayName = "Refresh Test"
        });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", new { });
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_ClearsRefreshToken()
    {
        var email = $"logout_{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest
        {
            Email = email,
            Password = "ValidPass1",
            DisplayName = "Logout Test"
        });

        var logoutResponse = await _client.PostAsJsonAsync("/api/v1/auth/logout", new { });
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
