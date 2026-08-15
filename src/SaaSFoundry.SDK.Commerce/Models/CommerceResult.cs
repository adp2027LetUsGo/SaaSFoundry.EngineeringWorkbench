using System.Collections.Generic;

namespace SaaSFoundry.SDK.Commerce.Models;

public enum CommerceErrorType
{
    AuthFailure,
    AuthorizationFailure,
    Validation,
    NotFound,
    Conflict,
    RateLimited,
    Transient,
    Permanent
}

public sealed class CommerceError
{
    public CommerceErrorType Type { get; }
    public string Message { get; }

    public CommerceError(CommerceErrorType type, string message)
    {
        Type = type;
        Message = message;
    }
}

public sealed class CommerceResult<T>
{
    public T? Data { get; }
    public bool IsSuccess { get; }
    public IReadOnlyList<CommerceError> Errors { get; }

    private CommerceResult(T? data, bool isSuccess, IReadOnlyList<CommerceError> errors)
    {
        Data = data;
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static CommerceResult<T> Success(T data) => new CommerceResult<T>(data, true, new List<CommerceError>());
    public static CommerceResult<T> Failure(IReadOnlyList<CommerceError> errors) => new CommerceResult<T>(default, false, errors);
    public static CommerceResult<T> Failure(CommerceError error) => new CommerceResult<T>(default, false, new[] { error });
}
