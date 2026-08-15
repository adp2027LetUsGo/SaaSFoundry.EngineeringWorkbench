using System;
using System.Collections.Generic;
using SaaSFoundry.SDK.Core.Diagnostics;
using SaaSFoundry.SDK.Core.Generators;

namespace SaaSFoundry.Plugins.Observability.Traceability;

public sealed class ObservabilityArtifactValidator : IArtifactValidator
{
    public IEnumerable<ValidationDiagnostic> Validate(GeneratedArtifactDescriptor desc)
    {
        var diagnostics = new List<ValidationDiagnostic>();
        
        if (!string.IsNullOrWhiteSpace(desc.CanonReference) && (!desc.CanonReference.StartsWith("OBS-", StringComparison.Ordinal) || desc.CanonReference.Length < 7))
        {
            diagnostics.Add(new ValidationDiagnostic(
                "ERR_INVALID_CANON_REF", 
                $"Invalid Canon reference '{desc.CanonReference}' on artifact '{desc.ArtifactId}'. Must conform to canonical format (e.g., OBS-003).", 
                IsError: true, 
                ArtifactId: desc.ArtifactId));
        }

        if (!string.IsNullOrWhiteSpace(desc.ImplementationReference) && (!desc.ImplementationReference.StartsWith("OBS-", StringComparison.Ordinal) || desc.ImplementationReference.Length < 7))
        {
            diagnostics.Add(new ValidationDiagnostic(
                "ERR_INVALID_IMPL_REF", 
                $"Invalid Implementation reference '{desc.ImplementationReference}' on artifact '{desc.ArtifactId}'. Must conform to canonical format (e.g., OBS-103).", 
                IsError: true, 
                ArtifactId: desc.ArtifactId));
        }
        
        return diagnostics;
    }
}

/// <summary>
/// Domain-specific wrapper for artifact generation.
/// </summary>
public sealed class ArtifactGenerator
{
    private readonly IArtifactGenerator _inner;

    public ArtifactGenerator(string pluginId = "observability", string pluginVersion = "1.0.0", string generatorVersion = "1.0.0")
    {
        _inner = new SaaSFoundry.SDK.Core.Generators.ArtifactGenerator(
            pluginId, 
            pluginVersion, 
            generatorVersion, 
            new[] { new ObservabilityArtifactValidator() });
    }

    public IReadOnlyList<ValidationDiagnostic> Validate(IEnumerable<GeneratedArtifactDescriptor> inputDescriptors, bool allowExternalDependencies = true)
    {
        return _inner.Validate(inputDescriptors, allowExternalDependencies);
    }

    public ArtifactGenerationResult Generate(IEnumerable<GeneratedArtifactDescriptor> inputDescriptors, long? timestampOverride = null, bool allowExternalDependencies = true, bool throwOnError = true)
    {
        return _inner.Generate(inputDescriptors, timestampOverride, allowExternalDependencies, throwOnError);
    }
}
