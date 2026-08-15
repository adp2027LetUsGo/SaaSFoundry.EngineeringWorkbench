using System;

namespace SaaSFoundry.SDK.Core.Results;

/// <summary>
/// Immutable result indicating success or failure.
/// </summary>
public readonly struct Result
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }

    private Result(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error ?? throw new ArgumentNullException(nameof(error)));
}
