using System;
using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation;

namespace SaaSFoundry.EngineeringWorkbench.Validation;

public sealed class ValidationEngine : IValidationEngine
{
    public ValidationReport AggregateAndValidate(IReadOnlyCollection<ValidationEvidence> evidence)
    {
        bool isSuccessful = evidence.All(e => e.IsSuccess);
        
        return new ValidationReport(
            Evidence: evidence,
            IsSuccessful: isSuccessful,
            GeneratedAt: DateTimeOffset.UtcNow
        );
    }
}
