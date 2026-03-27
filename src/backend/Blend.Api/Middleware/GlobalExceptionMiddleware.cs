using Blend.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Blend.Api.Middleware;

/// <summary>
/// Global exception handling middleware that converts unhandled exceptions
/// to RFC 9457 Problem Details responses.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items.TryGetValue("CorrelationId", out var cid)
                ? cid as string
                : context.TraceIdentifier;

            _logger.LogError(ex,
                "Unhandled exception [CorrelationId={CorrelationId}] {Method} {Path}",
                correlationId, context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex, correlationId, _environment.IsProduction());
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        string? correlationId,
        bool isProduction)
    {
        var (statusCode, title) = exception switch
        {
            Exceptions.ValidationException => (HttpStatusCode.BadRequest, "Validation failed"),
            Exceptions.UnauthorisedException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            Exceptions.ForbiddenException => (HttpStatusCode.Forbidden, "Forbidden"),
            Exceptions.NotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
            Exceptions.ConflictException => (HttpStatusCode.Conflict, "Conflict"),
            Exceptions.RateLimitException => (HttpStatusCode.TooManyRequests, "Too many requests"),
            ArgumentNullException => (HttpStatusCode.BadRequest, "Invalid argument"),
            ArgumentException => (HttpStatusCode.BadRequest, "Invalid argument"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            InvalidOperationException => (HttpStatusCode.UnprocessableEntity, "Invalid operation"),
            NotImplementedException => (HttpStatusCode.NotImplemented, "Not implemented"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        // Add Retry-After header for rate limit responses
        if (exception is Exceptions.RateLimitException rateLimitEx && rateLimitEx.RetryAfterSeconds.HasValue)
        {
            context.Response.Headers["Retry-After"] = rateLimitEx.RetryAfterSeconds.Value.ToString();
        }

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = isProduction && (int)statusCode == 500
                ? "An internal error occurred. Please try again later."
                : exception.Message,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        problemDetails.Extensions["correlationId"] = correlationId ?? context.TraceIdentifier;

        // Include field-level errors for validation exceptions
        if (exception is Exceptions.ValidationException validationEx && validationEx.Errors.Count > 0)
        {
            problemDetails.Extensions["errors"] = validationEx.Errors;
        }

        var json = System.Text.Json.JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }
}
