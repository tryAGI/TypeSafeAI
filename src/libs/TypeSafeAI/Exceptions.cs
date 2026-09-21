using System.Net;

namespace TypeSafeAI;

/// <summary>Base exception for TypeSafe API and transport failures.</summary>
public class TypeSafeException : Exception
{
    public TypeSafeException(string message, Exception? inner = null) : base(message, inner) { }
}

public class TypeSafeApiException : TypeSafeException
{
    internal TypeSafeApiException(string message, HttpStatusCode statusCode, string? responseBody, Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public HttpStatusCode StatusCode { get; }
    public string? ResponseBody { get; }
}

public sealed class BadRequestException(string message, string? body, Exception? inner = null)
    : TypeSafeApiException(message, HttpStatusCode.BadRequest, body, inner);
public sealed class AuthenticationException(string message, string? body, Exception? inner = null)
    : TypeSafeApiException(message, HttpStatusCode.Unauthorized, body, inner);
public sealed class PermissionDeniedException(string message, string? body, Exception? inner = null)
    : TypeSafeApiException(message, HttpStatusCode.Forbidden, body, inner);
public sealed class NotFoundException(string message, string? body, Exception? inner = null)
    : TypeSafeApiException(message, HttpStatusCode.NotFound, body, inner);
public sealed class UnprocessableEntityException(string message, string? body, Exception? inner = null)
    : TypeSafeApiException(message, HttpStatusCode.UnprocessableEntity, body, inner);
public sealed class RateLimitException(string message, string? body, TimeSpan? retryAfter, Exception? inner = null)
    : TypeSafeApiException(message, HttpStatusCode.TooManyRequests, body, inner)
{
    public TimeSpan? RetryAfter { get; } = retryAfter;
}
public sealed class InternalServerException(string message, HttpStatusCode statusCode, string? body, Exception? inner = null)
    : TypeSafeApiException(message, statusCode, body, inner);
public sealed class TypeSafeConnectionException(string message, Exception inner) : TypeSafeException(message, inner);
public sealed class TypeSafeTimeoutException(string message, Exception inner) : TypeSafeException(message, inner);
public sealed class TypeSafeUserAbortException(string message, Exception inner) : TypeSafeException(message, inner);
public sealed class TypeSafeResponseValidationException(string message, string? fieldPath = null, Exception? inner = null)
    : TypeSafeException(fieldPath is null ? message : $"{message} (at {fieldPath})", inner)
{
    public string? FieldPath { get; } = fieldPath;
}
