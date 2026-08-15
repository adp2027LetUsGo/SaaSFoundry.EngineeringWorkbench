using System;
using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.SDK.Core.Diagnostics;

namespace SaaSFoundry.SDK.Testing.Assertions;

public static class ValidationAssertions
{
    public static void AssertValid(IReadOnlyList<ValidationDiagnostic>? diagnostics)
    {
        if (diagnostics == null)
            throw new InvalidOperationException("Diagnostics collection is null.");

        var errors = diagnostics.Where(d => d.IsError).ToList();
        if (errors.Count > 0)
        {
            var msg = string.Join(", ", errors.Select(e => $"[{e.Code}] {e.Message}"));
            throw new InvalidOperationException($"Validation failed with errors: {msg}");
        }
    }

    public static void AssertHasError(IReadOnlyList<ValidationDiagnostic>? diagnostics, string errorCode)
    {
        if (diagnostics == null)
            throw new InvalidOperationException("Diagnostics collection is null.");

        if (!diagnostics.Any(d => d.IsError && string.Equals(d.Code, errorCode, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Expected validation error with code '{errorCode}' was not found.");
        }
    }
}
