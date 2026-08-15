using SaaSFoundry.SDK.Core.Generators;
using System;
using System.Linq;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.Plugins.Observability.Plugin;
using SaaSFoundry.Plugins.Observability.Traceability;
using SaaSFoundry.SDK.Plugins.Abstractions;
using Xunit;

namespace SaaSFoundry.EngineeringWorkbench.IntegrationTests;

public sealed class ObservabilityMilestone2IntegrationTests
{
    [Fact]
    public void Generate_Milestone2_Authoritative_Manifest_With_DependencyGraph_And_Diagnostics()
    {
        var plugin = new ObservabilityPlugin();
        Assert.Equal("observability", plugin.Manifest.Id);

        var milestone2Ids = new[] { "healthchecks", "collector", "configuration", "dashboards" };
        var milestone2Descriptors = milestone2Ids
            .Select(id => plugin.GetCapability(id))
            .Cast<ITraceablePluginCapability>()
            .SelectMany(cap => cap.GetArtifactDescriptors())
            .ToList();

        // 1. Verify exactly 16 additional artifacts in Milestone 2
        Assert.Equal(16, milestone2Descriptors.Count);

        var allCompletedIds = new[] { "logging", "metrics", "tracing", "healthchecks", "collector", "configuration", "dashboards" };
        var allDescriptors = allCompletedIds
            .Select(id => plugin.GetCapability(id))
            .Cast<ITraceablePluginCapability>()
            .SelectMany(cap => cap.GetArtifactDescriptors())
            .ToList();

        // Total 28 artifacts across Milestone 1 and Milestone 2
        Assert.Equal(28, allDescriptors.Count);

        var generator = new SaaSFoundry.Plugins.Observability.Traceability.ArtifactGenerator(plugin.Manifest.Id, plugin.Manifest.Version, "1.0.0");
        
        // When evaluating all completed capabilities together, strict closure (allowExternalDependencies: false) must pass with zero errors!
        var result = generator.Generate(allDescriptors, 1710000000000L, allowExternalDependencies: false, throwOnError: true);

        // 2. Verify diagnostics and manifest integrity
        Assert.NotNull(result.Diagnostics);
        Assert.DoesNotContain(result.Diagnostics!, d => d.IsError);
        
        var manifest = result.Manifest;
        Assert.NotNull(manifest);
        Assert.Equal("observability", manifest.PluginId);
        Assert.Equal(28, manifest.Artifacts.Count);
        Assert.Equal(28, manifest.TraceabilityRecords.Count);
        Assert.Equal(28, manifest.ValidationEvidence.Count);

        // 3. Verify dependency graph integrity
        Assert.NotNull(manifest.DependencyGraph);
        Assert.Equal(28, manifest.DependencyGraph!.Nodes.Count);
        
        var allArtifactIds = new HashSet<string>(manifest.Artifacts.Select(a => a.ArtifactId), StringComparer.OrdinalIgnoreCase);
        Assert.All(manifest.DependencyGraph.Nodes, node =>
        {
            Assert.Contains(node.ArtifactId, allArtifactIds);
            foreach (var dep in node.Dependencies)
            {
                Assert.Contains(dep, allArtifactIds);
            }
        });

        // Assert specific architectural dependency connections
        var dashboardNode = manifest.DependencyGraph.Nodes.Single(n => n.ArtifactId == "obs.dashboards.golden.json");
        Assert.Contains("obs.metrics.source.metrics", dashboardNode.Dependencies);
        Assert.Contains("obs.metrics.config.prometheus", dashboardNode.Dependencies);

        var collectorConfigNode = manifest.DependencyGraph.Nodes.Single(n => n.ArtifactId == "obs.collector.config");
        Assert.Contains("obs.configuration.appsettings", collectorConfigNode.Dependencies);
        Assert.Contains("obs.tracing.config.exporter", collectorConfigNode.Dependencies);
        Assert.Contains("obs.metrics.config.prometheus", collectorConfigNode.Dependencies);

        // 4. Verify traceability and category classifications
        Assert.All(manifest.TraceabilityRecords, trace =>
        {
            Assert.False(string.IsNullOrWhiteSpace(trace.CanonReference));
            Assert.False(string.IsNullOrWhiteSpace(trace.ImplementationReference));
            Assert.NotEqual(ArtifactCategory.Metadata, trace.ArtifactCategory);
            Assert.Equal(trace.ArtifactCategory.ToString(), trace.ArtifactType);
        });

        Assert.Equal(4, manifest.TraceabilityRecords.Count(r => r.CanonReference == "OBS-005" && r.ImplementationReference == "OBS-105"));
        Assert.Equal(4, manifest.TraceabilityRecords.Count(r => r.CanonReference == "OBS-006" && r.ImplementationReference == "OBS-106"));
        Assert.Equal(4, manifest.TraceabilityRecords.Count(r => r.CanonReference == "OBS-001" && r.ImplementationReference == "OBS-101"));
        Assert.Equal(4, manifest.TraceabilityRecords.Count(r => r.CanonReference == "OBS-007" && r.ImplementationReference == "OBS-107"));

        // Verify categories present in inventory
        var categories = manifest.Artifacts.Select(a => a.Category).Distinct().ToList();
        Assert.Contains(ArtifactCategory.SourceCode, categories);
        Assert.Contains(ArtifactCategory.Configuration, categories);
        Assert.Contains(ArtifactCategory.Documentation, categories);
        Assert.Contains(ArtifactCategory.Dashboard, categories);
        Assert.Contains(ArtifactCategory.Infrastructure, categories);
        Assert.Contains(ArtifactCategory.Evidence, categories);

        Assert.All(manifest.Artifacts, art =>
        {
            Assert.NotNull(art.Hash);
            Assert.StartsWith("SHA256:", art.Hash!);
            Assert.Contains(art.CanonReference, art.Content);
        });
    }
}
