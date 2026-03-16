using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace Blend.Tests.Integration;

public class OpenApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OpenApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Development");
        });
    }

    [Fact]
    public async Task GetOpenApiSpec_InDevelopment_Returns200()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOpenApiSpec_ReturnsValidOpenApiDocument()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");
        var content = await response.Content.ReadAsStringAsync();

        Assert.NotEmpty(content);
        Assert.Contains("openapi", content);
    }

    [Fact]
    public async Task GetOpenApiSpec_ReturnsJsonContentType()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Contains("json", response.Content.Headers.ContentType.MediaType ?? "");
    }
}
