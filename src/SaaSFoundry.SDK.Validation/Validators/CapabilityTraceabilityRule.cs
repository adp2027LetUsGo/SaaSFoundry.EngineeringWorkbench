using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.SDK.Core.Diagnostics;
using SaaSFoundry.SDK.Plugins.Abstractions;
using SaaSFoundry.SDK.Validation.Abstractions;

namespace SaaSFoundry.SDK.Validation.Validators;

public sealed class CapabilityTraceabilityRule : IValidationRule<IPluginCapability>
{
    public string RuleId => "RULE_TRC_CAPABILITY";
    public string Description => "Validates that traceable capabilities provide correct artifact traceability metadata.";

    public IEnumerable<ValidationDiagnostic> Validate(IPluginCapability context)
    {
        var diagnostics = new List<ValidationDiagnostic>();
        if (context == null) return diagnostics;

        if (context is ITraceablePluginCapability traceable)
        {
            var descriptors = traceable.GetArtifactDescriptors();
            if (descriptors != null)
            {
                foreach (var artifact in descriptors)
                {
                    if (string.IsNullOrWhiteSpace(artifact.CanonReference))
                    {
                        diagnostics.Add(new ValidationDiagnostic("RULE_TRC_001_CANON_REF", "Every artifact must specify a valid canonical architectural reference (e.g. OBS-001).", true, artifact.ArtifactId));
                    }
                    if (string.IsNullOrWhiteSpace(artifact.ImplementationReference))
                    {
                        diagnostics.Add(new ValidationDiagnostic("RULE_TRC_002_IMPL_REF", "Every artifact must specify a valid canonical implementation reference (e.g. OBS-101).", true, artifact.ArtifactId));
                    }
                    if (string.IsNullOrWhiteSpace(artifact.CapabilityId))
                    {
                        diagnostics.Add(new ValidationDiagnostic("RULE_TRC_003_CAPABILITY_OWNER", "Every artifact must designate a clear capability owner ID.", true, artifact.ArtifactId));
                    }
                    if (string.IsNullOrWhiteSpace(artifact.ValidationEvidenceId))
                    {
                        diagnostics.Add(new ValidationDiagnostic("RULE_TRC_004_VALIDATION_EVIDENCE", "Every artifact must link to a deterministic validation evidence identifier.", true, artifact.ArtifactId));
                    }
                }
            }
        }

        return diagnostics;
    }
}
