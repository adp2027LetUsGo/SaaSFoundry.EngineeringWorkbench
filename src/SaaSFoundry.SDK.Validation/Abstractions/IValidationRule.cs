using System.Collections.Generic;
using SaaSFoundry.SDK.Core.Diagnostics;

namespace SaaSFoundry.SDK.Validation.Abstractions;

/// <summary>
/// Represents a generic rule that validates a specific target context.
/// </summary>
public interface IValidationRule<in T>
{
    string RuleId { get; }
    string Description { get; }
    
    /// <summary>
    /// Validates the given context and returns a sequence of diagnostics.
    /// Returns an empty sequence if the rule passes without issues.
    /// </summary>
    IEnumerable<ValidationDiagnostic> Validate(T context);
}
