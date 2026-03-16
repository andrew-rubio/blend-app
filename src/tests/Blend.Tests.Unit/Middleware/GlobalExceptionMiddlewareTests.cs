using Blend.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Blend.Tests.Unit.Middleware;

public class GlobalExceptionMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionMiddleware>> _loggerMock;

    public GlobalExceptionMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/v1/test";
        return context;
    }

    private static async Task<ProblemDetails?> ReadProblemDetails(DefaultHttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return JsonSerializer.Deserialize<ProblemDetails>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_CallsNext()
    {
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_WhenArgumentException_Returns400()
    {
        RequestDelegate next = _ => throw new ArgumentException("Invalid argument value");
        var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(400, context.Response.StatusCode);
        var problem = await ReadProblemDetails(context);
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Equal("Invalid argument", problem.Title);
    }

    [Fact]
    public async Task InvokeAsync_WhenArgumentNullException_Returns400()
    {
        RequestDelegate next = _ => throw new ArgumentNullException("param");
        var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(400, context.Response.StatusCode);
        var problem = await ReadProblemDetails(context);
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task InvokeAsync_WhenKeyNotFoundException_Returns404()
    {
        RequestDelegate next = _ => throw new KeyNotFoundException("Resource not found");
        var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(404, context.Response.StatusCode);
        var problem = await ReadProblemDetails(context);
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.Equal("Resource not found", problem.Title);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthorizedAccessException_Returns401()
    {
        RequestDelegate next = _ => throw new UnauthorizedAccessException("Access denied");
        var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(401, context.Response.StatusCode);
        var problem = await ReadProblemDetails(context);
        Assert.NotNull(problem);
        Assert.Equal(401, problem.Status);
    }

    [Fact]
    public async Task InvokeAsync_WhenInvalidOperationException_Returns422()
    {
        RequestDelegate next = _ => throw new InvalidOperationException("Invalid state");
        var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(422, context.Response.StatusCode);
        var problem = await ReadProblemDetails(context);
        Assert.NotNull(problem);
        Assert.Equal(422, problem.Status);
    }

    [Fact]
    public async Task InvokeAsync_WhenNotImplementedException_Returns501()
    {
        RequestDelegate next = _ => throw new NotImplementedException("Not implemented yet");
        var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(501, context.Response.StatusCode);
        var problem = await ReadProblemDetails(context);
        Assert.NotNull(problem);
        Assert.Equal(501, problem.Status);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnhandledException_Returns500()
    {
        RequestDelegate next = _ => throw new Exception("Something went wrong");
        var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(500, context.Response.StatusCode);
        var problem = await ReadProblemDetails(context);
        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.Equal("An unexpected error occurred", problem.Title);
    }

    [Fact]
    public async Task InvokeAsync_WhenException_IncludesTraceIdInResponse()
    {
        RequestDelegate next = _ => throw new Exception("Error");
        var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();
        context.TraceIdentifier = "test-trace-id";

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("traceId", body);
    }

    [Fact]
    public async Task InvokeAsync_WhenException_SetsContentTypeToApplicationProblemJson()
    {
        RequestDelegate next = _ => throw new Exception("Error");
        var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.Equal("application/problem+json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_WhenException_IncludesInstancePath()
    {
        RequestDelegate next = _ => throw new Exception("Error");
        var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();
        context.Request.Path = "/api/v1/test";

        await middleware.InvokeAsync(context);

        var problem = await ReadProblemDetails(context);
        Assert.NotNull(problem);
        Assert.Equal("/api/v1/test", problem.Instance);
    }
}
