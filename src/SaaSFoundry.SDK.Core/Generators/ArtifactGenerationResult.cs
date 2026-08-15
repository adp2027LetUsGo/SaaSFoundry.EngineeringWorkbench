using System.Collections.Generic;
using SaaSFoundry.SDK.Core.Diagnostics;

namespace SaaSFoundry.SDK.Core.Generators;

/// <summary>
/// Represents the result of an artifact generation execution, encompassing generated descriptors, the manifest, traceability, and validation evidence.
/// </summary>
public sealed record ArtifactGenerationResult(
    IReadOnlyList<GeneratedArtifactDescriptor> GeneratedArtifacts,
    ArtifactManifest Manifest,
    IReadOnlyList<TraceabilityRecord> TraceabilityRecords,
    IReadOnlyList<ValidationEvidence> ValidationEvidence,
    string ExecutionSummary,
    IReadOnlyList<ValidationDiagnostic>? Diagnostics = null
);
