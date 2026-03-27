using Blend.Api.Middleware;
using Blend.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
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
    private readonly Mock<IHostEnvironment> _envMock;

    public GlobalExceptionMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        _envMock = new Mock<IHostEnvironment>();
        _envMock.Setup(e => e.EnvironmentName).Returns("Development");
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

    private GlobalExceptionMiddleware CreateMiddleware(RequestDelegate next) =>
        new(next, _loggerMock.Object, _envMock.Object);

    [Fact]
    public async Task InvokeAsync_WhenNoException_CallsNext()
    {
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_WhenArgumentException_Returns400()
    {
        RequestDelegate next = _ => throw new ArgumentException("Invalid argument value");
        var middleware = CreateMiddleware(next);
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
        var middleware = CreateMiddleware(next);
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
        var middleware = CreateMiddleware(next);
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
        var middleware = CreateMiddleware(next);
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
        var middleware = CreateMiddleware(next);
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
        var middleware = CreateMiddleware(next);
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
        var middleware = CreateMiddleware(next);
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
        var middleware = CreateMiddleware(next);
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
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.Equal("application/problem+json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_WhenException_IncludesInstancePath()
    {
        RequestDelegate next = _ => throw new Exception("Error");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();
        context.Request.Path = "/api/v1/test";

        await middleware.InvokeAsync(context);

        var problem = await ReadProblemDetails(context);
        Assert.NotNull(problem);
        Assert.Equal("/api/v1/test", problem.Instance);
    }

    // ── New domain exception tests ────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_WhenNotFoundException_Returns404()
    {
        RequestDelegate next = _ => throw new NotFoundException("Recipe", 123);
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(404, context.Response.StatusCode);
        var problem = await ReadProblemDetails(context);
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.Equal("Resource not found", problem.Title);
    }

    [Fact]
    public async Task InvokeAsync_WhenValidationException_Returns400()
    {
        RequestDelegate next = _ => throw new ValidationException("Name is required.");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(400, context.Response.StatusCode);
        var problem = await ReadProblemDetails(context);
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Equal("Validation failed", problem.Title);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthorisedException_Returns401()
    {
        RequestDelegate next = _ => throw new UnauthorisedException();
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenForbiddenException_Returns403()
    {
        RequestDelegate next = _ => throw new ForbiddenException();
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(403, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenConflictException_Returns409()
    {
        RequestDelegate next = _ => throw new ConflictException("Email already in use.");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(409, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenRateLimitException_Returns429()
    {
        RequestDelegate next = _ => throw new RateLimitException("Too many requests.", retryAfterSeconds: 30);
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(429, context.Response.StatusCode);
        Assert.Equal("30", context.Response.Headers["Retry-After"]);
    }

    [Fact]
    public async Task InvokeAsync_WhenException_IncludesCorrelationIdInResponse()
    {
        RequestDelegate next = _ => throw new Exception("Error");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();
        context.Items[CorrelationIdMiddleware.ItemKey] = "my-correlation-id";

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("correlationId", body);
    }

    [Fact]
    public async Task InvokeAsync_Production_Returns500WithGenericMessage()
    {
        var prodEnvMock = new Mock<IHostEnvironment>();
        prodEnvMock.Setup(e => e.EnvironmentName).Returns("Production");
        RequestDelegate next = _ => throw new Exception("Internal database error details");
        var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object, prodEnvMock.Object);
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(500, context.Response.StatusCode);
        var problem = await ReadProblemDetails(context);
        Assert.NotNull(problem);
        Assert.DoesNotContain("database error details", problem.Detail ?? "");
    }
}
