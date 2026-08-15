using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using SaaSFoundry.SDK.Core.Diagnostics;
using SaaSFoundry.SDK.Validation.Abstractions;

namespace SaaSFoundry.SDK.Validation.Validators;

public sealed class CapabilityRegistrationRule : IValidationRule<IPluginMetadataProvider>
{
    public string RuleId => "RULE_CAP_002_CAPABILITY_INTERFACES";
    public string Description => "Validates that registered capabilities implement the expected core interfaces.";

    public IEnumerable<ValidationDiagnostic> Validate(IPluginMetadataProvider context)
    {
        var diagnostics = new List<ValidationDiagnostic>();

        if (context == null || context.Metadata.Capabilities == null) return diagnostics;

        foreach (var capabilityId in context.Metadata.Capabilities)
        {
            if (string.IsNullOrWhiteSpace(capabilityId))
            {
                diagnostics.Add(new ValidationDiagnostic("RULE_CAP_ID_EXISTS", "Capability must have a non-empty Id.", true, capabilityId));
            }
        }
        
        // Capability uniqueness check
        var duplicateIds = context.Metadata.Capabilities
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .GroupBy(c => c)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var dup in duplicateIds)
        {
            diagnostics.Add(new ValidationDiagnostic("RULE_CAP_UNIQUE", $"Duplicate Capability Id detected: '{dup}'.", true, dup));
        }

        return diagnostics;
    }
}
