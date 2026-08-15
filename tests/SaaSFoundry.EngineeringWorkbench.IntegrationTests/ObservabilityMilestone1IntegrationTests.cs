using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.Plugins.Observability.Plugin;
using SaaSFoundry.Plugins.Observability.Traceability;
using SaaSFoundry.SDK.Plugins.Abstractions;
using Xunit;

namespace SaaSFoundry.EngineeringWorkbench.IntegrationTests;

public sealed class ObservabilityMilestone1IntegrationTests
{
    [Fact]
    public void Generate_Milestone1_Authoritative_Manifest_And_Inventory()
    {
        var plugin = new ObservabilityPlugin();
        Assert.Equal("observability", plugin.Manifest.Id);

        var coreSignalIds = new[] { "logging", "metrics", "tracing" };
        var allDescriptors = coreSignalIds
            .Select(id => plugin.GetCapability(id))
            .Cast<ITraceablePluginCapability>()
            .SelectMany(cap => cap.GetArtifactDescriptors())
            .ToList();

        Assert.Equal(12, allDescriptors.Count);

        var generator = new SaaSFoundry.Plugins.Observability.Traceability.ArtifactGenerator(plugin.Manifest.Id, plugin.Manifest.Version, "1.0.0");
        var result = generator.Generate(allDescriptors, 1710000000000L);

        // 1. Verify complete manifest generation
        var manifest = result.Manifest;
        Assert.NotNull(manifest);
        Assert.Equal("observability", manifest.PluginId);
        Assert.Equal("1.0.0", manifest.PluginVersion);
        Assert.Equal(12, manifest.Artifacts.Count);
        Assert.Equal(12, manifest.TraceabilityRecords.Count);
        Assert.Equal(12, manifest.ValidationEvidence.Count);

        // 2. Verify complete traceability
        Assert.All(manifest.TraceabilityRecords, trace =>
        {
            Assert.False(string.IsNullOrWhiteSpace(trace.CanonReference));
            Assert.False(string.IsNullOrWhiteSpace(trace.ImplementationReference));
            Assert.False(string.IsNullOrWhiteSpace(trace.ArtifactId));
            Assert.False(string.IsNullOrWhiteSpace(trace.ValidationEvidenceId));
            Assert.Contains(trace.CapabilityId, coreSignalIds);
        });

        Assert.Equal(4, manifest.TraceabilityRecords.Count(r => r.CanonReference == "OBS-003" && r.ImplementationReference == "OBS-103"));
        Assert.Equal(4, manifest.TraceabilityRecords.Count(r => r.CanonReference == "OBS-004" && r.ImplementationReference == "OBS-104"));
        Assert.Equal(4, manifest.TraceabilityRecords.Count(r => r.CanonReference == "OBS-002" && r.ImplementationReference == "OBS-102"));

        // 3. Verify generation summary
        Assert.False(string.IsNullOrWhiteSpace(result.ExecutionSummary));
        Assert.Contains("Successfully generated 12 deterministic artifacts", result.ExecutionSummary);

        // 4. Verify artifact inventory integrity and types
        var artifactTypes = manifest.TraceabilityRecords.Select(r => r.ArtifactType).Distinct().ToList();
        Assert.Contains("Configuration", artifactTypes);
        Assert.Contains("SourceCode", artifactTypes);
        Assert.Contains("Documentation", artifactTypes);
        Assert.Contains("Evidence", artifactTypes);

        Assert.All(manifest.Artifacts, art =>
        {
            Assert.NotNull(art.Hash);
            Assert.StartsWith("SHA256:", art.Hash!);
            Assert.False(string.IsNullOrWhiteSpace(art.Content));
            Assert.Contains(art.CanonReference, art.Content);
        });
    }
}
