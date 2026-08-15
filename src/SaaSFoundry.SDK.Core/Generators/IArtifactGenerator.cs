using System.Collections.Generic;
using SaaSFoundry.SDK.Core.Diagnostics;

namespace SaaSFoundry.SDK.Core.Generators;

/// <summary>
/// Represents a generic capability to generate engineering artifacts deterministically.
/// </summary>
public interface IArtifactGenerator
{
    IReadOnlyList<ValidationDiagnostic> Validate(IEnumerable<GeneratedArtifactDescriptor> inputDescriptors, bool allowExternalDependencies = true);

    ArtifactGenerationResult Generate(
        IEnumerable<GeneratedArtifactDescriptor> inputDescriptors, 
        long? timestampOverride = null, 
        bool allowExternalDependencies = true, 
        bool throwOnError = true);
}
