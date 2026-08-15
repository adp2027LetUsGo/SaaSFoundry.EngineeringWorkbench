using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.SDK.Core.Diagnostics;
using SaaSFoundry.SDK.Validation.Abstractions;

namespace SaaSFoundry.SDK.Validation.Validators;

public sealed class CapabilityGovernanceRule : IValidationRule<IPluginCapability>
{
    public string RuleId => "RULE_GOV_CAPABILITY";
    public string Description => "Validates that governed capabilities provide correct governance metadata.";

    public IEnumerable<ValidationDiagnostic> Validate(IPluginCapability context)
    {
        var diagnostics = new List<ValidationDiagnostic>();
        if (context == null) return diagnostics;

        if (context is IGovernedPluginCapability governed)
        {
            var meta = governed.GovernanceMetadata;
            if (meta == null)
            {
                diagnostics.Add(new ValidationDiagnostic("RULE_GOV_000_METADATA_EXISTS", "Governed capability must return non-null GovernanceMetadata.", true, context.Id));
                return diagnostics;
            }

            if (meta.Risk == RiskLevel.None)
            {
                diagnostics.Add(new ValidationDiagnostic("RULE_GOV_001_RISK_LEVEL", "Every governed capability must explicitly declare a risk level higher than None.", true, context.Id));
            }

            if (meta.RequiredPermissions == null || !meta.RequiredPermissions.Any())
            {
                diagnostics.Add(new ValidationDiagnostic("RULE_GOV_002_PERMISSIONS", "Every governed capability must explicitly declare required execution permissions.", true, context.Id));
            }

            if (meta.ValidationRequirements == null || !meta.ValidationRequirements.Any())
            {
                diagnostics.Add(new ValidationDiagnostic("RULE_GOV_003_VALIDATION_REQUIREMENTS", "Every governed capability must explicitly declare governance validation requirements.", true, context.Id));
            }
        }

        return diagnostics;
    }
}
