using System.Collections.Generic;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation;
using SaaSFoundry.SDK.Core.Generators;
using SaaSFoundry.SDK.Core.Diagnostics;

namespace SaaSFoundry.SDK.Packaging.Models;

/// <summary>
/// Immutable record representing a portable engineering delivery unit produced by SaaSFoundry.
/// Captures all generated artifacts, authoritative manifests, traceability linkage, validation evidence, and dependency hierarchy.
/// </summary>
public sealed record EngineeringPackageDescriptor(
    string PackageId,
    string PluginId,
    string PluginVersion,
    string GeneratorVersion,
    long CreationTimestamp,
    string PackageDescription,
    ArtifactManifest Manifest,
    IReadOnlyList<GeneratedArtifactDescriptor> Artifacts,
    IReadOnlyList<TraceabilityRecord> TraceabilityRecords,
    IReadOnlyList<SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation.ValidationEvidence> ValidationEvidence,
    ArtifactDependencyGraph DependencyGraph,
    string PackageHash
);
