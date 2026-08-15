using SaaSFoundry.SDK.Core.Generators;
using System;
using System.Linq;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.Plugins.Observability.Capabilities.Validation;
using SaaSFoundry.Plugins.Observability.Planner;
using SaaSFoundry.Plugins.Observability.Plugin;
using SaaSFoundry.Plugins.Observability.Traceability;
using SaaSFoundry.SDK.Plugins.Abstractions;
using Xunit;

namespace SaaSFoundry.EngineeringWorkbench.IntegrationTests;

public sealed class ObservabilityEngineeringPackageGenerationTest
{
    [Fact]
    public void Generate_Complete_Authoritative_Engineering_Package_Across_Seven_Capabilities()
    {
        var plugin = new ObservabilityPlugin();
        Assert.Equal("observability", plugin.Manifest.Id);
        Assert.Equal("1.0.0", plugin.Manifest.Version);

        // 1. Verify exactly 7 completed capabilities producing 28 artifacts
        var completedCapabilityIds = new[] 
        { 
            "logging", "metrics", "tracing", 
            "healthchecks", "collector", "configuration", "dashboards" 
        };

        var capabilities = completedCapabilityIds
            .Select(id => plugin.GetCapability(id))
            .Cast<ITraceablePluginCapability>()
            .ToList();

        Assert.Equal(7, capabilities.Count);

        var allDescriptors = capabilities
            .SelectMany(cap => cap.GetArtifactDescriptors())
            .ToList();

        Assert.Equal(28, allDescriptors.Count);

        // 2. Perform deterministic artifact generation with strict closure enforcement
        var generator = new SaaSFoundry.Plugins.Observability.Traceability.ArtifactGenerator(plugin.Manifest.Id, plugin.Manifest.Version, "1.0.0");
        const long targetTimestamp = 1720000000000L;
        var result = generator.Generate(allDescriptors, targetTimestamp, allowExternalDependencies: false, throwOnError: true);

        Assert.NotNull(result.Diagnostics);
        Assert.DoesNotContain(result.Diagnostics!, d => d.IsError);

        // 3. Assemble and validate immutable Engineering Package
        var builder = new SaaSFoundry.SDK.Packaging.Builders.PackageBuilder(plugin.Manifest.Id, plugin.Manifest.Version, "1.0.0");
        var packageResult = builder.Build("pkg-saasfoundry-observability-v1", "Authoritative Observability Engineering Delivery Unit", result, targetTimestamp);

        // 4. Validate package structure, inventory, and hash
        Assert.True(packageResult.IsSuccess);
        var package = packageResult.Package;
        Assert.NotNull(package);
        Assert.Equal("pkg-saasfoundry-observability-v1", package.PackageId);
        Assert.Equal("observability", package.PluginId);
        Assert.Equal("1.0.0", package.PluginVersion);
        Assert.Equal("1.0.0", package.GeneratorVersion);
        Assert.Equal(targetTimestamp, package.CreationTimestamp);
        Assert.NotNull(package.PackageHash);
        Assert.StartsWith("SHA256:", package.PackageHash);

        // Assert exact counts in portable delivery package
        Assert.Equal(28, package.Artifacts.Count);
        Assert.Equal(28, package.TraceabilityRecords.Count);
        Assert.Equal(28, package.ValidationEvidence.Count);
        Assert.NotNull(package.Manifest);
        Assert.NotNull(package.DependencyGraph);
        Assert.Equal(28, package.DependencyGraph.Nodes.Count);

        // 5. Verify explicit separation of concepts:
        // Capability dependencies (WHAT executes) vs Artifact dependencies (WHAT is generated)
        var planTasks = ObservabilityPlanContributor.GetRequestedTasks();
        var metricsTask = planTasks.Single(t => t.CapabilityId == "metrics");
        Assert.Single(metricsTask.Dependencies);
        Assert.Equal("logging", metricsTask.Dependencies[0].CapabilityId); // Execution sequence constraint

        // Whereas artifact dependencies define generation and wiring relationships
        var dashboardArtifactNode = package.DependencyGraph.Nodes.Single(n => n.ArtifactId == "obs.dashboards.golden.json");
        Assert.Contains("obs.metrics.source.metrics", dashboardArtifactNode.Dependencies);
        Assert.Contains("obs.metrics.config.prometheus", dashboardArtifactNode.Dependencies);
        Assert.DoesNotContain(dashboardArtifactNode.Dependencies, d => d == "logging"); // Artifact does not directly bind logging file

        // 6. Ensure package validation passes cleanly without diagnostics errors
        var packageDiagnostics = builder.Validate(result);
        Assert.DoesNotContain(packageDiagnostics, d => d.IsError);

        // 7. Verify all artifacts possess verified evidence and explicit category taxonomy
        Assert.All(package.Artifacts, art =>
        {
            Assert.NotNull(art.Hash);
            Assert.StartsWith("SHA256:", art.Hash!);
            Assert.NotEqual(ArtifactCategory.Metadata, art.Category);
            Assert.Contains(art.CanonReference, art.Content);
        });

        Assert.All(package.ValidationEvidence, ev =>
        {
            Assert.True(ev.IsSuccess);
            Assert.Equal("observability", ev.PluginId);
        });

        // 8. Test package hash determinism across independent runs
        var result2 = generator.Generate(allDescriptors, targetTimestamp, allowExternalDependencies: false);
        var packageResult2 = builder.Build("pkg-saasfoundry-observability-v1", "Authoritative Observability Engineering Delivery Unit", result2, targetTimestamp);
        Assert.True(packageResult2.IsSuccess);
        Assert.Equal(package.PackageHash, packageResult2.Package!.PackageHash);
    }
}
