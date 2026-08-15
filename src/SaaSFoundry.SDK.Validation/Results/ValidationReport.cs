using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.SDK.Core.Diagnostics;

namespace SaaSFoundry.SDK.Validation.Results;

/// <summary>
/// Immutable structured validation report containing deterministic diagnostics.
/// </summary>
public sealed class ValidationReport
{
    public bool IsValid { get; }
    public IReadOnlyList<ValidationDiagnostic> Diagnostics { get; }
    public IReadOnlyList<string> FailedRules { get; }

    public ValidationReport(IEnumerable<ValidationDiagnostic> diagnostics)
    {
        // Diagnostics are ordered deterministically:
        // By Code (RuleId) -> ArtifactId -> IsError -> Message
        var orderedDiagnostics = diagnostics
            .OrderBy(d => d.Code)
            .ThenBy(d => d.ArtifactId ?? string.Empty)
            .ThenByDescending(d => d.IsError)
            .ThenBy(d => d.Message)
            .ToList();

        Diagnostics = orderedDiagnostics;
        FailedRules = orderedDiagnostics
            .Where(d => d.IsError)
            .Select(d => d.Code)
            .Distinct()
            .ToList();
            
        IsValid = FailedRules.Count == 0;
    }
}
