using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Blend.Tests.Integration;

public class ExceptionHandlingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ExceptionHandlingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Development");
        });
    }

    [Fact]
    public async Task RequestToUnknownRoute_Returns404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/nonexistent-route-that-does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
