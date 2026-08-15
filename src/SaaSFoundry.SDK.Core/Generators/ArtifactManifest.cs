using System.Collections.Generic;
using SaaSFoundry.SDK.Core.Diagnostics;

namespace SaaSFoundry.SDK.Core.Generators;

/// <summary>
/// Authoritative engineering inventory manifest representing the complete list of artifacts produced by an execution.
/// </summary>
public sealed record ArtifactManifest(
    string PluginId,
    string PluginVersion,
    long GenerationTime,
    string GeneratorVersion,
    IReadOnlyList<GeneratedArtifactDescriptor> Artifacts,
    IReadOnlyList<TraceabilityRecord> TraceabilityRecords,
    IReadOnlyList<ValidationEvidence> ValidationEvidence,
    ArtifactDependencyGraph? DependencyGraph = null
);
