namespace Blend.Api.Exceptions;

/// <summary>
/// Thrown when a requested resource cannot be found (maps to HTTP 404).
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string resource, object id) : base($"{resource} '{id}' was not found.") { }
}

/// <summary>
/// Thrown when request data fails validation (maps to HTTP 400).
/// </summary>
public sealed class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = new Dictionary<string, string[]>(errors);
    }
}

/// <summary>
/// Thrown when the current user is not authenticated (maps to HTTP 401).
/// </summary>
public sealed class UnauthorisedException : Exception
{
    public UnauthorisedException(string message = "Authentication is required.") : base(message) { }
}

/// <summary>
/// Thrown when the current user lacks permission to access a resource (maps to HTTP 403).
/// </summary>
public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message = "You do not have permission to perform this action.") : base(message) { }
}

/// <summary>
/// Thrown when a resource already exists and cannot be duplicated (maps to HTTP 409).
/// </summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>
/// Thrown when the caller has exceeded the allowed request rate (maps to HTTP 429).
/// </summary>
public sealed class RateLimitException : Exception
{
    /// <summary>Number of seconds after which the caller may retry.</summary>
    public int? RetryAfterSeconds { get; }

    public RateLimitException(string message = "Too many requests. Please try again later.", int? retryAfterSeconds = null)
        : base(message)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}
