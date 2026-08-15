using System;
using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.SDK.Core.Generators;
using SaaSFoundry.SDK.Core.Diagnostics;

namespace SaaSFoundry.SDK.Testing.Assertions;

public static class TraceabilityAssertions
{
    public static void AssertCompleteCoverage(IReadOnlyList<GeneratedArtifactDescriptor>? artifacts, IReadOnlyList<TraceabilityRecord>? records)
    {
        if (artifacts == null)
            throw new InvalidOperationException("Artifacts collection is null.");

        if (records == null)
            throw new InvalidOperationException("Traceability records collection is null.");

        if (artifacts.Count != records.Count)
            throw new InvalidOperationException($"Coverage mismatch: {artifacts.Count} artifacts but {records.Count} traceability records.");

        var artifactIds = artifacts.Select(a => a.ArtifactId).ToHashSet(StringComparer.Ordinal);
        var recordArtifactIds = records.Select(r => r.ArtifactId).ToHashSet(StringComparer.Ordinal);

        if (!artifactIds.SetEquals(recordArtifactIds))
            throw new InvalidOperationException("Mismatch between artifact IDs and traceability record artifact IDs.");
    }
}
