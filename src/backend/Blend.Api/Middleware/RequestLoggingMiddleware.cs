using System.Diagnostics;
using System.Security.Claims;

namespace Blend.Api.Middleware;

/// <summary>
/// Middleware that logs structured request/response metadata for every HTTP request.
/// Captured context: correlation ID, user ID (if authenticated), HTTP method, path,
/// response status, and request duration (per PLAT-53 through PLAT-56).
/// Request/response bodies are intentionally NOT logged unless opt-in debug logging is enabled.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var correlationId = context.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var cid)
                ? cid as string
                : context.TraceIdentifier;

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms " +
                "[CorrelationId={CorrelationId}, UserId={UserId}]",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId,
                userId ?? "anonymous");
        }
    }
}
