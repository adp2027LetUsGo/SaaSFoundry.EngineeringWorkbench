using System;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation;

public sealed record ValidationEvidence(
    string PluginId,
    string CapabilityId,
    string Stage,
    bool IsSuccess,
    string Message,
    DateTimeOffset Timestamp
);

public sealed record ValidationReport(
    System.Collections.Generic.IReadOnlyCollection<ValidationEvidence> Evidence,
    bool IsSuccessful,
    DateTimeOffset GeneratedAt
);

public interface IValidationEngine
{
    ValidationReport AggregateAndValidate(System.Collections.Generic.IReadOnlyCollection<ValidationEvidence> evidence);
}
