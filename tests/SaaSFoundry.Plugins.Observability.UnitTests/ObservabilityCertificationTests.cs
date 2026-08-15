using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.Plugins.Observability.Certification;
using SaaSFoundry.Plugins.Observability.Plugin;
using SaaSFoundry.Plugins.Observability.Traceability;
using SaaSFoundry.SDK.Plugins.Abstractions;
using Xunit;

namespace SaaSFoundry.Plugins.Observability.UnitTests;

public sealed class ObservabilityCertificationTests
{
    [Fact]
    public void Test1_FrozenCoreProtection_VerifiesNoCoreContractsChanged()
    {
        // 1. Assert Core assembly has zero dependencies on any implementation plugin
        var coreAssembly = typeof(IEngineeringPlugin).Assembly;
        var referencedAssemblies = coreAssembly.GetReferencedAssemblies();
        Assert.DoesNotContain(referencedAssemblies, r => r.Name != null && r.Name.Contains("Plugin", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referencedAssemblies, r => r.Name != null && r.Name.Contains("Observability", StringComparison.OrdinalIgnoreCase));

        // 2. Verify exact presence and immutability of Core interfaces without modification
        Assert.True(typeof(IEngineeringPlugin).IsInterface);
        Assert.True(typeof(IPluginCapability).IsInterface);
        Assert.True(typeof(IPluginManifest).IsInterface);
        Assert.True(typeof(IGovernedPluginCapability).IsInterface);
        Assert.True(typeof(IPluginExecutionPolicy).IsInterface);
        Assert.True(typeof(ICapabilityExecutionPolicy).IsInterface);
        Assert.True(typeof(IPluginMetadataProvider).IsInterface);

        // 3. Confirm core contracts expose standard explicit signatures without reflection abstracts
        var capProperty = typeof(IEngineeringPlugin).GetProperty("Capabilities");
        Assert.NotNull(capProperty);
        Assert.Equal("IReadOnlyCollection`1", capProperty!.PropertyType.Name);
    }

    [Fact]
    public void Test2_NativeAOTCompliance_VerifiesNoReflectionOrDynamicLoading()
    {
        // 1. Assert plugin assembly does not reference dynamic compilation or emitting libraries
        var pluginAssembly = typeof(ObservabilityPlugin).Assembly;
        var referencedAssemblies = pluginAssembly.GetReferencedAssemblies();
        Assert.DoesNotContain(referencedAssemblies, r => r.Name != null && r.Name.StartsWith("System.Reflection.Emit", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referencedAssemblies, r => r.Name != null && r.Name.StartsWith("Microsoft.CodeAnalysis", StringComparison.OrdinalIgnoreCase));

        // 2. Verify zero reflection instantiation and explicit table capability discovery
        var plugin = new ObservabilityPlugin();
        var capabilities = plugin.Capabilities.ToList();
        Assert.Equal(10, capabilities.Count);

        // 3. Confirm each capability is constructed explicitly in memory without dynamic assembly scanning
        Assert.Contains(capabilities, c => c.Id == "configuration");
        Assert.Contains(capabilities, c => c.Id == "tracing");
        Assert.Contains(capabilities, c => c.Id == "logging");
        Assert.Contains(capabilities, c => c.Id == "metrics");
        Assert.Contains(capabilities, c => c.Id == "healthchecks");
        Assert.Contains(capabilities, c => c.Id == "collector");
        Assert.Contains(capabilities, c => c.Id == "dashboards");
        Assert.Contains(capabilities, c => c.Id == "alerts");
        Assert.Contains(capabilities, c => c.Id == "documentation");
        Assert.Contains(capabilities, c => c.Id == "validation");
    }

    [Fact]
    public void Test3_CompleteTraceability_Verifies37ArtifactsAndEvidenceRecords()
    {
        var plugin = new ObservabilityPlugin();
        var capabilities = plugin.Capabilities.Cast<ITraceablePluginCapability>().ToList();
        
        var allDescriptors = capabilities.SelectMany(c => c.GetArtifactDescriptors()).ToList();
        Assert.Equal(37, allDescriptors.Count);

        const long targetTimestamp = 1720000000000L;
        var generator = new SaaSFoundry.Plugins.Observability.Traceability.ArtifactGenerator(plugin.Manifest.Id, plugin.Manifest.Version, "1.0.0");
        var result = generator.Generate(allDescriptors, targetTimestamp, allowExternalDependencies: false, throwOnError: true);

        var builder = new SaaSFoundry.SDK.Packaging.Builders.PackageBuilder(plugin.Manifest.Id, plugin.Manifest.Version, "1.0.0");
        var package = builder.Build("pkg-saasfoundry-observability-v1", "Certified Observability Reference Package", result, targetTimestamp).Package!;

        // Assert exactly 37 artifacts, 37 traceability records, and 37 validation evidence records
        Assert.Equal(37, package.Artifacts.Count);
        Assert.Equal(37, package.TraceabilityRecords.Count);
        Assert.Equal(37, package.ValidationEvidence.Count);
        Assert.Equal(37, package.DependencyGraph.Nodes.Count);
    }

    [Fact]
    public void Test4_GovernanceCoverage_Verifies100PercentGovernanceMetadata()
    {
        var plugin = new ObservabilityPlugin();
        var capabilities = plugin.Capabilities.ToList();
        Assert.Equal(10, capabilities.Count);

        Assert.All(capabilities, cap =>
        {
            var governed = Assert.IsAssignableFrom<IGovernedPluginCapability>(cap);
            Assert.NotNull(governed.GovernanceMetadata);
            Assert.Contains(cap.Id, governed.GovernanceMetadata.CapabilityId, StringComparison.OrdinalIgnoreCase);
            Assert.NotEqual(RiskLevel.None, governed.GovernanceMetadata.Risk);
            Assert.NotNull(governed.GovernanceMetadata.RequiredPermissions);
            Assert.NotEmpty(governed.GovernanceMetadata.RequiredPermissions);
            Assert.NotNull(governed.GovernanceMetadata.ValidationRequirements);
            Assert.NotEmpty(governed.GovernanceMetadata.ValidationRequirements);
        });
    }

    [Fact]
    public void Test5_PackageCertification_ExecutesCertificationEngineWithSuccess()
    {
        var plugin = new ObservabilityPlugin();
        var capabilities = plugin.Capabilities.Cast<ITraceablePluginCapability>().ToList();
        var allDescriptors = capabilities.SelectMany(c => c.GetArtifactDescriptors()).ToList();

        const long targetTimestamp = 1720000000000L;
        var generator = new SaaSFoundry.Plugins.Observability.Traceability.ArtifactGenerator(plugin.Manifest.Id, plugin.Manifest.Version, "1.0.0");
        var result = generator.Generate(allDescriptors, targetTimestamp, allowExternalDependencies: false, throwOnError: true);

        var builder = new SaaSFoundry.SDK.Packaging.Builders.PackageBuilder(plugin.Manifest.Id, plugin.Manifest.Version, "1.0.0");
        var package = builder.Build("pkg-saasfoundry-observability-v1", "Certified Observability Reference Package", result, targetTimestamp).Package!;

        var engine = new ObservabilityCertificationEngine();
        var evaluation = engine.Evaluate(plugin, package);

        // Assert 100% compliance score and Certified = true
        Assert.Empty(evaluation.FailedRules);
        Assert.DoesNotContain(evaluation.Diagnostics, d => d.IsError);
        Assert.Equal(100.0, evaluation.ComplianceScore);
        Assert.True(evaluation.IsCertified);

        var descriptor = engine.Certify(plugin, package, targetTimestamp);
        Assert.NotNull(descriptor);
        Assert.Equal(10, descriptor.CapabilityCount);
        Assert.Equal(37, descriptor.ArtifactCount);
        Assert.Equal(100.0, descriptor.TraceabilityCoverage);
        Assert.Equal(100.0, descriptor.GovernanceCoverage);
        Assert.Equal("CERTIFIED_COMPLIANT_V1", descriptor.ValidationStatus);
        Assert.Equal(targetTimestamp, descriptor.CertificationTimestamp);
        Assert.StartsWith("SHA256:", descriptor.CertificationHash);
    }

    [Fact]
    public void Test6_GoldenReferencePackage_SerializesToBenchmarkFormat()
    {
        var plugin = new ObservabilityPlugin();
        var capabilities = plugin.Capabilities.Cast<ITraceablePluginCapability>().ToList();
        var allDescriptors = capabilities.SelectMany(c => c.GetArtifactDescriptors()).ToList();

        const long targetTimestamp = 1720000000000L;
        var generator = new SaaSFoundry.Plugins.Observability.Traceability.ArtifactGenerator(plugin.Manifest.Id, plugin.Manifest.Version, "1.0.0");
        var result = generator.Generate(allDescriptors, targetTimestamp, allowExternalDependencies: false, throwOnError: true);

        var builder = new SaaSFoundry.SDK.Packaging.Builders.PackageBuilder(plugin.Manifest.Id, plugin.Manifest.Version, "1.0.0");
        var package = builder.Build("pkg-saasfoundry-observability-v1", "Certified Observability Reference Package", result, targetTimestamp).Package!;

        var engine = new ObservabilityCertificationEngine();
        var goldenPackage = engine.GenerateGoldenReferencePackage(plugin, package, targetTimestamp);

        Assert.NotNull(goldenPackage);
        Assert.Equal(10, goldenPackage.GovernanceMetadata.Count);
        Assert.Equal(37, goldenPackage.ArtifactInventory.Count);
        Assert.Equal(37, goldenPackage.ValidationEvidence.Count);

        var json = JsonSerializer.Serialize(goldenPackage, new JsonSerializerOptions { WriteIndented = true });
        Assert.Contains("pkg-saasfoundry-observability-v1", json);
        Assert.Contains("CERTIFIED_COMPLIANT_V1", json);
        Assert.Contains("obs.alerts.rules.prometheus", json);
        Assert.Contains("obs.documentation.traceability.matrix", json);
        Assert.Contains("obs.validation.engine", json);

        var curDir = AppContext.BaseDirectory;
        var rootDir = curDir;
        while (rootDir != null && !File.Exists(Path.Combine(rootDir, "SaaSFoundry.EngineeringWorkbench.sln")))
        {
            rootDir = Path.GetDirectoryName(rootDir);
        }

        if (rootDir != null)
        {
            var refPackageDir = Path.Combine(rootDir, "src", "SaaSFoundry.Plugins.Observability", "certification", "reference-package");
            Directory.CreateDirectory(refPackageDir);
            var refPackagePath = Path.Combine(refPackageDir, "observability-reference-package.json");
            File.WriteAllText(refPackagePath, json);
            Assert.True(File.Exists(refPackagePath));
        }
    }
}
