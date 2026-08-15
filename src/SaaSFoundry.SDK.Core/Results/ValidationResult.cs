using System;
using System.Collections.Generic;
using System.Linq;

namespace SaaSFoundry.SDK.Core.Results;

/// <summary>
/// Represents the result of a validation operation with potential multiple errors.
/// </summary>
public sealed record ValidationResult
{
    public bool IsSuccess => !Errors.Any();
    public IReadOnlyList<ValidationError> Errors { get; }

    public ValidationResult(IReadOnlyList<ValidationError>? errors = null)
    {
        Errors = errors ?? Array.Empty<ValidationError>();
    }

    public static ValidationResult Success() => new(Array.Empty<ValidationError>());
    public static ValidationResult Failure(IReadOnlyList<ValidationError> errors) => new(errors);
}
