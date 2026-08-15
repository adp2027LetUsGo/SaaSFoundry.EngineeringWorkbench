using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.SDK.Core.Generators;
using SaaSFoundry.SDK.Core.Diagnostics;
using SaaSFoundry.SDK.Validation.Abstractions;

namespace SaaSFoundry.SDK.Validation.Validators;

public sealed class ArtifactInvariantsRule : IValidationRule<IReadOnlyList<GeneratedArtifactDescriptor>>
{
    public string RuleId => "RULE_ART_INVARIANTS";
    public string Description => "Validates artifact invariants including uniqueness and deterministic identity.";

    public IEnumerable<ValidationDiagnostic> Validate(IReadOnlyList<GeneratedArtifactDescriptor> context)
    {
        var diagnostics = new List<ValidationDiagnostic>();
        if (context == null) return diagnostics;

        // Duplicate ArtifactId
        var duplicateIds = context
            .GroupBy(a => a.ArtifactId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var dup in duplicateIds)
        {
            diagnostics.Add(new ValidationDiagnostic("RULE_ART_001_DUPLICATE_ID", $"Duplicate artifact ID discovered: '{dup}'.", true, dup));
        }

        // Duplicate Output Path
        var duplicatePaths = context
            .GroupBy(a => a.RelativePath)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var dup in duplicatePaths)
        {
            diagnostics.Add(new ValidationDiagnostic("RULE_ART_002_DUPLICATE_PATH", $"Duplicate artifact relative path discovered: '{dup}'.", true));
        }

        // Invalid dependencies
        var allIds = new HashSet<string>(context.Select(a => a.ArtifactId));
        foreach (var artifact in context)
        {
            if (artifact.Dependencies != null)
            {
                foreach (var dep in artifact.Dependencies)
                {
                    // Check if dependency exists in the list
                    // Since SDK plugins might depend on artifacts not in the same capability,
                    // we can only validate strict closure if required. The Observability ArtifactGenerator
                    // allows external dependencies. We'll add a warning if it's missing, but it could be valid externally.
                    if (!allIds.Contains(dep))
                    {
                        // In STAGE 1, ArtifactGenerator does this validation:
                        // "Artifact '{art.ArtifactId}' declares dependency on '{dep}' which is not in the generation set."
                        // We will flag it as an error because the rule says "Missing dependency when strict closure applies"
                        // But since we are just validating the descriptors statically, we can flag it.
                        // Actually, ArtifactGenerator allows allowExternalDependencies=true.
                        // We will emit it as a warning here because we don't know the strict closure setting.
                        diagnostics.Add(new ValidationDiagnostic("RULE_ART_003_MISSING_DEP", $"Artifact '{artifact.ArtifactId}' declares a dependency '{dep}' which is not present in the provided context.", false, artifact.ArtifactId));
                    }
                }
            }
        }

        return diagnostics;
    }
}
