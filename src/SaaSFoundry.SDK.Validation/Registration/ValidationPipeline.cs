using System.Collections.Generic;
using SaaSFoundry.SDK.Core.Diagnostics;
using SaaSFoundry.SDK.Validation.Abstractions;
using SaaSFoundry.SDK.Validation.Results;

namespace SaaSFoundry.SDK.Validation.Registration;

/// <summary>
/// A composition pipeline to evaluate explicitly registered validation rules against a context.
/// </summary>
public sealed class ValidationPipeline<T>
{
    private readonly IReadOnlyList<IValidationRule<T>> _rules;

    public ValidationPipeline(params IValidationRule<T>[] rules)
    {
        _rules = rules;
    }

    /// <summary>
    /// Executes all rules against the provided context and produces a deterministic ValidationReport.
    /// </summary>
    public ValidationReport Validate(T context)
    {
        var diagnostics = new List<ValidationDiagnostic>();
        foreach (var rule in _rules)
        {
            var ruleDiagnostics = rule.Validate(context);
            if (ruleDiagnostics != null)
            {
                diagnostics.AddRange(ruleDiagnostics);
            }
        }
        return new ValidationReport(diagnostics);
    }
}
