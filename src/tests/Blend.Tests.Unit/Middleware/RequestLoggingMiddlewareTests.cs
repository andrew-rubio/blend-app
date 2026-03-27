using Blend.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Blend.Tests.Unit.Middleware;

public class RequestLoggingMiddlewareTests
{
    private readonly Mock<ILogger<RequestLoggingMiddleware>> _loggerMock;

    public RequestLoggingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<RequestLoggingMiddleware>>();
    }

    private static DefaultHttpContext CreateHttpContext(string method = "GET", string path = "/api/v1/test")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Response.Body = new System.IO.MemoryStream();
        return context;
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new RequestLoggingMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_LogsRequestInformation()
    {
        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        };

        var middleware = new RequestLoggingMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext("POST", "/api/v1/recipes");

        await middleware.InvokeAsync(context);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString()!.Contains("POST") &&
                    v.ToString()!.Contains("/api/v1/recipes")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LogsCorrelationIdWhenPresent()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new RequestLoggingMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();
        context.Items[CorrelationIdMiddleware.ItemKey] = "test-correlation-id";

        await middleware.InvokeAsync(context);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("test-correlation-id")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LogsAnonymousWhenNoUser()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new RequestLoggingMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("anonymous")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LogsUserIdWhenAuthenticated()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new RequestLoggingMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "user-123")],
            "TestAuth"));

        await middleware.InvokeAsync(context);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("user-123")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_StillLogsWhenNextThrows()
    {
        RequestDelegate next = _ => throw new Exception("Oops");
        var middleware = new RequestLoggingMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        await Assert.ThrowsAsync<Exception>(() => middleware.InvokeAsync(context));

        // Logging should still happen in the finally block
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
