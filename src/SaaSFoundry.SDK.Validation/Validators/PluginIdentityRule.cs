using System.Collections.Generic;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using SaaSFoundry.SDK.Core.Diagnostics;
using SaaSFoundry.SDK.Validation.Abstractions;

namespace SaaSFoundry.SDK.Validation.Validators;

public sealed class PluginIdentityRule : IValidationRule<IPluginMetadataProvider>
{
    public string RuleId => "RULE_ID_001_PLUGIN_IDENTITY";
    public string Description => "Validates that the plugin provides a valid Identity (PluginId, Version, Fingerprint).";

    public IEnumerable<ValidationDiagnostic> Validate(IPluginMetadataProvider context)
    {
        var diagnostics = new List<ValidationDiagnostic>();
        
        if (context == null)
        {
            diagnostics.Add(new ValidationDiagnostic(RuleId, "Plugin metadata provider cannot be null.", true));
            return diagnostics;
        }

        if (string.IsNullOrWhiteSpace(context.Identity.PluginId))
        {
            diagnostics.Add(new ValidationDiagnostic("RULE_ID_001_PLUGIN_ID_EXISTS", "Plugin must provide a valid non-empty PluginId in its Identity.", true));
        }

        if (string.IsNullOrWhiteSpace(context.Identity.Version))
        {
            diagnostics.Add(new ValidationDiagnostic("RULE_ID_002_VERSION_EXISTS", "Plugin must provide a valid non-empty Version in its Identity.", true));
        }

        if (string.IsNullOrWhiteSpace(context.Identity.Fingerprint))
        {
            diagnostics.Add(new ValidationDiagnostic("RULE_ID_003_FINGERPRINT_EXISTS", "Plugin must provide a valid non-empty Fingerprint in its Identity.", true));
        }

        return diagnostics;
    }
}
