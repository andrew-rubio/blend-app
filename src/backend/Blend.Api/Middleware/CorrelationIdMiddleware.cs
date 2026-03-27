namespace Blend.Api.Middleware;

/// <summary>
/// Middleware that propagates a correlation ID through every request.
/// Reads <c>X-Correlation-Id</c> from the incoming request; if absent, generates a new GUID.
/// The correlation ID is stored in <c>HttpContext.Items["CorrelationId"]</c> and echoed
/// back in the <c>X-Correlation-Id</c> response header.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemKey = "CorrelationId";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items[ItemKey] = correlationId;
        context.TraceIdentifier = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
