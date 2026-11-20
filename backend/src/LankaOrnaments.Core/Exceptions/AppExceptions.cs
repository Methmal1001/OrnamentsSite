namespace LankaOrnaments.Core.Exceptions;

/// <summary>Base type for expected errors that map to a ProblemDetails response.</summary>
public abstract class AppException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public sealed class NotFoundException(string message) : AppException(message, 404);

public sealed class ConflictException(string message) : AppException(message, 409);

public sealed class ForbiddenException(string message) : AppException(message, 403);

public sealed class UnauthorizedAppException(string message) : AppException(message, 401);

/// <summary>Field-level validation errors (serialised as ValidationProblemDetails).</summary>
public sealed class RequestValidationException : AppException
{
    public RequestValidationException(IDictionary<string, string[]> errors, string message = "One or more validation errors occurred.")
        : base(message, 400)
    {
        Errors = new Dictionary<string, string[]>(errors);
    }

    public RequestValidationException(string field, string error)
        : this(new Dictionary<string, string[]> { [field] = [error] }, error)
    {
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
