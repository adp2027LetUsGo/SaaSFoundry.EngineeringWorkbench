using System;
using System.Linq;
using SaaSFoundry.Plugins.Observability.Capabilities.Logging;
using SaaSFoundry.Plugins.Observability.Traceability;
using SaaSFoundry.SDK.Packaging.Models;
using Xunit;

namespace SaaSFoundry.Plugins.Observability.UnitTests;

public sealed class EngineeringPackageDescriptorTests
{
    [Fact]
    public void EngineeringPackageDescriptor_IsImmutable_AndPreservesMetadata()
    {
        var logging = new LoggingCapability();
        var descriptors = logging.GetArtifactDescriptors();

        var generator = new SaaSFoundry.Plugins.Observability.Traceability.ArtifactGenerator("observability", "1.0.0", "1.0.0");
        var genResult = generator.Generate(descriptors, 1715000000000L, allowExternalDependencies: true);

        var builder = new SaaSFoundry.SDK.Packaging.Builders.PackageBuilder("observability", "1.0.0", "1.0.0");
        var package = builder.Build("pkg-obs-log-v1", "Reference logging package", genResult, 1715000000000L).Package!;

        // Verify package metadata
        Assert.Equal("pkg-obs-log-v1", package.PackageId);
        Assert.Equal("observability", package.PluginId);
        Assert.Equal("1.0.0", package.PluginVersion);
        Assert.Equal("1.0.0", package.GeneratorVersion);
        Assert.Equal(1715000000000L, package.CreationTimestamp);
        Assert.Equal("Reference logging package", package.PackageDescription);
        Assert.NotNull(package.PackageHash);
        Assert.StartsWith("SHA256:", package.PackageHash);

        // Verify artifact inventory and structural containment
        Assert.Same(genResult.Manifest, package.Manifest);
        Assert.Same(genResult.GeneratedArtifacts, package.Artifacts);
        Assert.Same(genResult.TraceabilityRecords, package.TraceabilityRecords);
        // Verify evidence was mapped correctly
        Assert.Equal(genResult.ValidationEvidence.Count, package.ValidationEvidence.Count);
        for (int i = 0; i < genResult.ValidationEvidence.Count; i++)
        {
            Assert.Equal(genResult.ValidationEvidence[i].PluginId, package.ValidationEvidence[i].PluginId);
            Assert.Equal(genResult.ValidationEvidence[i].CapabilityId, package.ValidationEvidence[i].CapabilityId);
            Assert.Equal(genResult.ValidationEvidence[i].Stage, package.ValidationEvidence[i].Stage);
            Assert.Equal(genResult.ValidationEvidence[i].IsSuccess, package.ValidationEvidence[i].IsSuccess);
        }
        Assert.Same(genResult.Manifest.DependencyGraph, package.DependencyGraph);

        Assert.Equal(4, package.Artifacts.Count);
        Assert.Equal(4, package.TraceabilityRecords.Count);
        Assert.Equal(4, package.ValidationEvidence.Count);

        // Verify traceability linkage preservation
        Assert.All(package.TraceabilityRecords, trace =>
        {
            Assert.Equal("OBS-003", trace.CanonReference);
            Assert.Equal("OBS-103", trace.ImplementationReference);
            Assert.Equal("logging", trace.CapabilityId);
            Assert.Contains(package.Artifacts, a => a.ArtifactId == trace.ArtifactId);
        });
    }

    [Fact]
    public void EngineeringPackageBuilder_CalculatesDeterministicPackageHash()
    {
        var logging = new LoggingCapability();
        var descriptors = logging.GetArtifactDescriptors();

        var generator = new SaaSFoundry.Plugins.Observability.Traceability.ArtifactGenerator("observability", "1.0.0", "1.0.0");
        var genResult1 = generator.Generate(descriptors, 1715000000000L);
        var genResult2 = generator.Generate(descriptors, 1715000000000L);

        var builder = new SaaSFoundry.SDK.Packaging.Builders.PackageBuilder("test-plugin", "1.0.0", "1.0.0");
        var pkg1 = builder.Build("pkg-obs-log", "Desc", genResult1, 1715000000000L).Package!;
        var pkg2 = builder.Build("pkg-obs-log", "Desc", genResult2, 1715000000000L).Package!;

        Assert.Equal(pkg1.PackageHash, pkg2.PackageHash);
        Assert.Equal(pkg1.PackageId, pkg2.PackageId);
        Assert.Equal(pkg1.Artifacts.Count, pkg2.Artifacts.Count);
        
        for (int i = 0; i < pkg1.Artifacts.Count; i++)
        {
            Assert.Equal(pkg1.Artifacts[i].Hash, pkg2.Artifacts[i].Hash);
        }

        // Altering Package ID changes the resulting package hash deterministically
        var pkg3 = builder.Build("pkg-obs-log-modified", "Desc", genResult1, 1715000000000L).Package!;
        Assert.NotEqual(pkg1.PackageHash, pkg3.PackageHash);
    }
}
