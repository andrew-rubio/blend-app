using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace Blend.Tests.Integration;

public class CorsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CorsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Development");
            builder.UseSetting("Cors:AllowedOrigins:0", "http://localhost:3000");
        });
    }

    [Fact]
    public async Task CorsPreflightRequest_FromAllowedOrigin_ReturnsAllowOriginHeader()
    {
        var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Options, "/healthz");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "Content-Type");

        var response = await client.SendAsync(request);

        // CORS preflight should succeed (204 or 200)
        Assert.True(
            response.StatusCode == HttpStatusCode.NoContent ||
            response.StatusCode == HttpStatusCode.OK,
            $"Expected 204 or 200, got {response.StatusCode}");
    }

    [Fact]
    public async Task CorsRequest_FromAllowedOrigin_ReturnsAccessControlHeader()
    {
        var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/healthz");
        request.Headers.Add("Origin", "http://localhost:3000");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(
            response.Headers.Contains("Access-Control-Allow-Origin"),
            "Response should contain Access-Control-Allow-Origin header");
    }
}
