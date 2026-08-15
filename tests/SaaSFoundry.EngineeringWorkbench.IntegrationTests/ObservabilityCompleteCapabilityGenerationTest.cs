using System;
using System.Linq;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.Plugins.Observability.Capabilities.Validation;
using SaaSFoundry.Plugins.Observability.Planner;
using SaaSFoundry.Plugins.Observability.Plugin;
using SaaSFoundry.Plugins.Observability.Traceability;
using SaaSFoundry.SDK.Plugins.Abstractions;
using SaaSFoundry.SDK.Testing.Assertions;
using Xunit;

namespace SaaSFoundry.EngineeringWorkbench.IntegrationTests;

public sealed class ObservabilityCompleteCapabilityGenerationTest
{
    [Fact]
    public void Generate_Complete_Ten_Capabilities_ThirtySeven_Artifacts_Package_And_Verify_Compliance()
    {
        var plugin = new ObservabilityPlugin();
        Assert.Equal("observability", plugin.Manifest.Id);
        Assert.Equal("1.0.0", plugin.Manifest.Version);

        // 1. Verify 10 / 10 capabilities implemented as ITraceablePluginCapability
        var capabilityIds = new[] 
        { 
            "logging", "metrics", "tracing", "healthchecks", 
            "collector", "configuration", "dashboards",
            "alerts", "documentation", "validation"
        };

        var capabilities = capabilityIds
            .Select(id => plugin.GetCapability(id))
            .Cast<ITraceablePluginCapability>()
            .ToList();

        Assert.Equal(10, capabilities.Count);

        // 2. Collect complete artifact inventory across all 10 capabilities
        var allDescriptors = capabilities
            .SelectMany(cap => cap.GetArtifactDescriptors())
            .ToList();

        // 28 previous + 3 alerts + 3 documentation + 3 validation = 37 authoritative artifacts
        Assert.Equal(37, allDescriptors.Count);

        // 3. Perform deterministic artifact generation with strict dependency graph closure
        var generator = new SaaSFoundry.Plugins.Observability.Traceability.ArtifactGenerator(plugin.Manifest.Id, plugin.Manifest.Version, "1.0.0");
        const long targetTimestamp = 1725000000000L;
        
        // Strict closure verification: allowExternalDependencies = false ensures no broken references or cycles
        var result = generator.Generate(allDescriptors, targetTimestamp, allowExternalDependencies: false, throwOnError: true);
        
        ValidationAssertions.AssertValid(result.Diagnostics);
        Assert.NotNull(result.Manifest);
        Assert.Equal(37, result.Manifest.Artifacts.Count);

        // 4. Assemble into immutable Engineering Package
        var builder = new SaaSFoundry.SDK.Packaging.Builders.PackageBuilder(plugin.Manifest.Id, plugin.Manifest.Version, "1.0.0");
        var packageResult = builder.Build("pkg-saasfoundry-observability-complete-v1", "Authoritative Complete Observability Suite Package", result, targetTimestamp);
        
        PackagingAssertions.AssertValidPackage(packageResult);
        var package = packageResult.Package;

        // 5. Verify Package structure and deterministic cryptographic hash
        Assert.NotNull(package);
        Assert.Equal("pkg-saasfoundry-observability-complete-v1", package.PackageId);
        Assert.NotNull(package.PackageHash);
        Assert.StartsWith("SHA256:", package.PackageHash);
        Assert.Equal(37, package.Artifacts.Count);
        Assert.Equal(37, package.TraceabilityRecords.Count);
        Assert.Equal(37, package.ValidationEvidence.Count);
        Assert.Equal(37, package.DependencyGraph.Nodes.Count);

        // 6. Execute formal compliance validation using ValidationCapability as compliance authority
        var validationCapability = (ValidationCapability)capabilities.Single(c => c is ValidationCapability);
        var complianceDiagnostics = validationCapability.VerifyCompliance(package!);
        ValidationAssertions.AssertValid(complianceDiagnostics);

        // 7. Verify new operational capabilities dependency bindings in the DAG
        var alertsNode = package.DependencyGraph.Nodes.Single(n => n.ArtifactId == "obs.alerts.rules.prometheus");
        Assert.Contains("obs.metrics.config.prometheus", alertsNode.Dependencies);

        var docsNode = package.DependencyGraph.Nodes.Single(n => n.ArtifactId == "obs.documentation.traceability.matrix");
        Assert.Contains("obs.alerts.rules.prometheus", docsNode.Dependencies);

        var validationEngineNode = package.DependencyGraph.Nodes.Single(n => n.ArtifactId == "obs.validation.engine");
        Assert.Contains("obs.documentation.traceability.matrix", validationEngineNode.Dependencies);

        // 8. Assert all evidence records are fully verified and successful
        Assert.All(package.ValidationEvidence, ev => Assert.True(ev.IsSuccess));
    }
}
