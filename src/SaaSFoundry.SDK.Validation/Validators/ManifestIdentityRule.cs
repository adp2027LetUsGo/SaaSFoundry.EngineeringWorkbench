using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.SDK.Core.Diagnostics;
using SaaSFoundry.SDK.Validation.Abstractions;

namespace SaaSFoundry.SDK.Validation.Validators;

public sealed class ManifestIdentityRule : IValidationRule<IPluginManifest>
{
    public string RuleId => "RULE_MAN_001_IDENTITY";
    public string Description => "Validates that the plugin manifest provides complete identity and compatibility data.";

    public IEnumerable<ValidationDiagnostic> Validate(IPluginManifest context)
    {
        var diagnostics = new List<ValidationDiagnostic>();
        if (context == null)
        {
            diagnostics.Add(new ValidationDiagnostic(RuleId, "Manifest cannot be null.", true));
            return diagnostics;
        }

        if (string.IsNullOrWhiteSpace(context.Id))
        {
            diagnostics.Add(new ValidationDiagnostic("RULE_MAN_001_PLUGIN_ID", "Manifest must specify PluginId.", true));
        }

        if (string.IsNullOrWhiteSpace(context.Name))
        {
            diagnostics.Add(new ValidationDiagnostic("RULE_MAN_002_NAME", "Manifest must specify Name.", true));
        }

        if (string.IsNullOrWhiteSpace(context.Version))
        {
            diagnostics.Add(new ValidationDiagnostic("RULE_MAN_003_VERSION", "Manifest must specify Version.", true));
        }

        if (string.IsNullOrWhiteSpace(context.Description))
        {
            diagnostics.Add(new ValidationDiagnostic("RULE_MAN_004_DESCRIPTION", "Manifest must specify Description.", true));
        }

        if (context.Compatibility == null || !context.Compatibility.Any())
        {
            diagnostics.Add(new ValidationDiagnostic("RULE_MAN_005_COMPATIBILITY", "Manifest must specify at least one CompatibilityTarget.", true, context.Id));
        }

        return diagnostics;
    }
}
