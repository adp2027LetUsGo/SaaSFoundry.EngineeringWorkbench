using System;
using SaaSFoundry.SDK.Core.Generators;

namespace SaaSFoundry.SDK.Core.Diagnostics;

/// <summary>
/// Represents immutable engineering traceability linking canonical architecture to implementation library artifacts.
/// </summary>
public sealed record TraceabilityRecord(
    string CanonReference,
    string ImplementationReference,
    string CapabilityId,
    string ArtifactId,
    string ArtifactType,
    string ArtifactName,
    string ValidationEvidenceId,
    long GenerationTimestamp,
    string GeneratorVersion,
    string Notes,
    ArtifactCategory ArtifactCategory = ArtifactCategory.Metadata
);
