using System.Collections.Generic;
using SaaSFoundry.SDK.Core.Diagnostics;
using SaaSFoundry.SDK.Core.Generators;
using SaaSFoundry.SDK.Packaging.Results;

namespace SaaSFoundry.SDK.Packaging.Abstractions;

/// <summary>
/// Public contract for deterministic package authoring composition from generation results.
/// </summary>
public interface IPackageBuilder
{
    IReadOnlyList<ValidationDiagnostic> Validate(ArtifactGenerationResult? result);
    PackagePreparationResult Build(string packageId, string packageDescription, ArtifactGenerationResult result, long? timestampOverride = null);
}
