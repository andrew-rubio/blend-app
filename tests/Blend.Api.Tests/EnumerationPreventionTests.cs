using System.Net;
using System.Net.Http.Json;
using Blend.Api.Models.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Blend.Api.Tests;

public class EnumerationPreventionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EnumerationPreventionTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Jwt:SecretKey", "test-secret-key-at-least-32-chars-long-!!!");
            builder.UseSetting("Jwt:Issuer", "test");
            builder.UseSetting("Jwt:Audience", "test");
        }).CreateClient();
    }

    [Fact]
    public async Task ForgotPassword_AlwaysReturns200_ForUnknownEmail()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password",
            new ForgotPasswordRequest { Email = "notexist@nowhere.com" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ForgotPassword_AlwaysReturns200_ForKnownEmail()
    {
        await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest
        {
            Email = "enum@test.com",
            Password = "ValidPass1",
            DisplayName = "Enum Test"
        });

        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password",
            new ForgotPasswordRequest { Email = "enum@test.com" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_ReturnsGenericError_ForNonExistentUser()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest { Email = "ghost@nowhere.com", Password = "SomePass1" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("ghost@nowhere.com");
    }

    [Fact]
    public async Task Login_ReturnsGenericError_ForWrongPassword()
    {
        await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest
        {
            Email = "real@test.com",
            Password = "ValidPass1",
            DisplayName = "Real User"
        });

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest { Email = "real@test.com", Password = "WrongPass1" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
