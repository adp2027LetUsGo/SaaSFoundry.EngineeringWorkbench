using SaaSFoundry.SDK.Core.Diagnostics;
using SaaSFoundry.SDK.Core.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SaaSFoundry.Plugins.Observability.Capabilities.Collector;
using SaaSFoundry.Plugins.Observability.Capabilities.Configuration;
using SaaSFoundry.Plugins.Observability.Capabilities.Dashboards;
using SaaSFoundry.Plugins.Observability.Capabilities.HealthChecks;
using SaaSFoundry.Plugins.Observability.Capabilities.Logging;
using SaaSFoundry.Plugins.Observability.Capabilities.Metrics;
using SaaSFoundry.Plugins.Observability.Capabilities.Tracing;
using SaaSFoundry.Plugins.Observability.Traceability;
using SaaSFoundry.SDK.Plugins.Abstractions;
using Xunit;

namespace SaaSFoundry.Plugins.Observability.UnitTests;

public sealed class TraceabilityInfrastructureTests
{
    [Fact]
    public void TraceabilityRecord_IsImmutableAndValid()
    {
        var record = new TraceabilityRecord(
            CanonReference: "OBS-003",
            ImplementationReference: "OBS-103",
            CapabilityId: "logging",
            ArtifactId: "obs.logging.config.json",
            ArtifactType: "Configuration",
            ArtifactName: "logging.json",
            ValidationEvidenceId: "ev.obs.logging.config",
            GenerationTimestamp: 1700000000000L,
            GeneratorVersion: "1.0.0",
            Notes: "Test record"
        );

        Assert.Equal("OBS-003", record.CanonReference);
        Assert.Equal("OBS-103", record.ImplementationReference);
        Assert.Equal("logging", record.CapabilityId);
        Assert.Equal("Configuration", record.ArtifactType);
        Assert.Equal("logging.json", record.ArtifactName);
        Assert.Equal("1.0.0", record.GeneratorVersion);
        Assert.Equal(ArtifactCategory.Metadata, record.ArtifactCategory);
    }

    [Fact]
    public void EngineeringArtifactCategory_EnumHasExpectedMembers_AndBindsToRecords()
    {
        Assert.True(Enum.IsDefined(typeof(ArtifactCategory), ArtifactCategory.SourceCode));
        Assert.True(Enum.IsDefined(typeof(ArtifactCategory), ArtifactCategory.Dashboard));
        Assert.True(Enum.IsDefined(typeof(ArtifactCategory), ArtifactCategory.Infrastructure));
        Assert.True(Enum.IsDefined(typeof(ArtifactCategory), ArtifactCategory.Evidence));

        var descriptor = new GeneratedArtifactDescriptor(
            ArtifactId: "test.id",
            FileName: "test.json",
            RelativePath: "test.json",
            ContentType: "application/json",
            CapabilityId: "test",
            CanonReference: "OBS-001",
            ImplementationReference: "OBS-101",
            Description: "Test descriptor",
            Generator: "Test v1.0",
            ValidationEvidenceId: "ev.test",
            Content: "{}",
            Category: ArtifactCategory.Dashboard,
            Dependencies: new[] { "dep.id" }
        );

        Assert.Equal(ArtifactCategory.Dashboard, descriptor.Category);
        Assert.Single(descriptor.Dependencies ?? Array.Empty<string>());
        Assert.Equal("dep.id", descriptor.Dependencies![0]);
    }

    [Fact]
    public void CoreSignalCapabilities_ExposeTraceableDescriptors_WithCorrectObsReferences()
    {
        var logging = new LoggingCapability();
        var metrics = new MetricsCapability();
        var tracing = new TracingCapability();

        Assert.All(new ITraceablePluginCapability[] { logging, metrics, tracing }, cap =>
        {
            var descriptors = cap.GetArtifactDescriptors();
            Assert.Equal(4, descriptors.Count);
            Assert.All(descriptors, d =>
            {
                Assert.False(string.IsNullOrWhiteSpace(d.ArtifactId));
                Assert.False(string.IsNullOrWhiteSpace(d.FileName));
                Assert.False(string.IsNullOrWhiteSpace(d.ContentType));
                Assert.False(string.IsNullOrWhiteSpace(d.Content));
                Assert.False(string.IsNullOrWhiteSpace(d.ValidationEvidenceId));
                Assert.Equal("ObservabilityPlugin v1.0.0", d.Generator);
            });
        });

        Assert.All(logging.GetArtifactDescriptors(), d =>
        {
            Assert.Equal("logging", d.CapabilityId);
            Assert.Equal("OBS-003", d.CanonReference);
            Assert.Equal("OBS-103", d.ImplementationReference);
        });

        Assert.All(metrics.GetArtifactDescriptors(), d =>
        {
            Assert.Equal("metrics", d.CapabilityId);
            Assert.Equal("OBS-004", d.CanonReference);
            Assert.Equal("OBS-104", d.ImplementationReference);
        });

        Assert.All(tracing.GetArtifactDescriptors(), d =>
        {
            Assert.Equal("tracing", d.CapabilityId);
            Assert.Equal("OBS-002", d.CanonReference);
            Assert.Equal("OBS-102", d.ImplementationReference);
        });
    }

    [Fact]
    public void Milestone2Capabilities_ExposeTraceableDescriptors_WithCorrectObsReferencesAndCategories()
    {
        var healthchecks = new HealthChecksCapability();
        var collector = new CollectorCapability();
        var configuration = new ConfigurationCapability();
        var dashboards = new DashboardsCapability();

        Assert.All(new ITraceablePluginCapability[] { healthchecks, collector, configuration, dashboards }, cap =>
        {
            var descriptors = cap.GetArtifactDescriptors();
            Assert.Equal(4, descriptors.Count);
            Assert.All(descriptors, d =>
            {
                Assert.False(string.IsNullOrWhiteSpace(d.ArtifactId));
                Assert.False(string.IsNullOrWhiteSpace(d.FileName));
                Assert.False(string.IsNullOrWhiteSpace(d.ContentType));
                Assert.False(string.IsNullOrWhiteSpace(d.Content));
                Assert.False(string.IsNullOrWhiteSpace(d.ValidationEvidenceId));
                Assert.Equal("ObservabilityPlugin v1.0.0", d.Generator);
                Assert.NotEqual(ArtifactCategory.Metadata, d.Category); // Every descriptor must explicitly assign a category
            });
        });

        Assert.All(healthchecks.GetArtifactDescriptors(), d =>
        {
            Assert.Equal("healthchecks", d.CapabilityId);
            Assert.Equal("OBS-005", d.CanonReference);
            Assert.Equal("OBS-105", d.ImplementationReference);
        });

        Assert.All(collector.GetArtifactDescriptors(), d =>
        {
            Assert.Equal("collector", d.CapabilityId);
            Assert.Equal("OBS-006", d.CanonReference);
            Assert.Equal("OBS-106", d.ImplementationReference);
        });

        Assert.All(configuration.GetArtifactDescriptors(), d =>
        {
            Assert.Equal("configuration", d.CapabilityId);
            Assert.Equal("OBS-001", d.CanonReference);
            Assert.Equal("OBS-101", d.ImplementationReference);
        });

        Assert.All(dashboards.GetArtifactDescriptors(), d =>
        {
            Assert.Equal("dashboards", d.CapabilityId);
            Assert.Equal("OBS-007", d.CanonReference);
            Assert.Equal("OBS-107", d.ImplementationReference);
        });

        // Verify explicit dependency relationship examples from prompt
        var dashboardsGolden = dashboards.GetArtifactDescriptors().Single(d => d.ArtifactId == "obs.dashboards.golden.json");
        Assert.Contains("obs.metrics.source.metrics", dashboardsGolden.Dependencies!);

        var collectorConfig = collector.GetArtifactDescriptors().Single(d => d.ArtifactId == "obs.collector.config");
        Assert.Contains("obs.tracing.config.exporter", collectorConfig.Dependencies!);
    }

    [Fact]
    public void ArtifactGenerator_GeneratesDeterministically_WithValidHashesAndManifest()
    {
        var logging = new LoggingCapability();
        var metrics = new MetricsCapability();
        var tracing = new TracingCapability();

        var allDescriptors = logging.GetArtifactDescriptors()
            .Concat(metrics.GetArtifactDescriptors())
            .Concat(tracing.GetArtifactDescriptors())
            .ToList();

        Assert.Equal(12, allDescriptors.Count);

        var generator = new SaaSFoundry.Plugins.Observability.Traceability.ArtifactGenerator("observability", "1.0.0", "1.0.0");
        const long timestamp = 1712345678900L;

        var run1 = generator.Generate(allDescriptors, timestamp);
        var run2 = generator.Generate(allDescriptors, timestamp);

        Assert.Equal(12, run1.GeneratedArtifacts.Count);
        Assert.Equal(run1.GeneratedArtifacts.Count, run2.GeneratedArtifacts.Count);
        for (int i = 0; i < run1.GeneratedArtifacts.Count; i++)
        {
            Assert.Equal(run1.GeneratedArtifacts[i].ArtifactId, run2.GeneratedArtifacts[i].ArtifactId);
            Assert.Equal(run1.GeneratedArtifacts[i].Hash, run2.GeneratedArtifacts[i].Hash);
            Assert.StartsWith("SHA256:", run1.GeneratedArtifacts[i].Hash!);
        }

        var manifest = run1.Manifest;
        Assert.NotNull(manifest);
        Assert.Equal("observability", manifest.PluginId);
        Assert.Equal("1.0.0", manifest.PluginVersion);
        Assert.Equal(timestamp, manifest.GenerationTime);
        Assert.Equal(12, manifest.Artifacts.Count);
        Assert.Equal(12, manifest.TraceabilityRecords.Count);
        Assert.Equal(12, manifest.ValidationEvidence.Count);
        Assert.NotNull(manifest.DependencyGraph);
        Assert.Equal(12, manifest.DependencyGraph!.Nodes.Count);

        Assert.All(manifest.ValidationEvidence, e =>
        {
            Assert.True(e.IsSuccess);
            Assert.Equal("observability", e.PluginId);
        });
    }

    [Fact]
    public void ArtifactGenerator_ThrowsOnDuplicateArtifactIds()
    {
        var logging = new LoggingCapability();
        var duplicateDescriptors = logging.GetArtifactDescriptors().Concat(logging.GetArtifactDescriptors()).ToList();

        var generator = new SaaSFoundry.Plugins.Observability.Traceability.ArtifactGenerator();
        var exception = Assert.Throws<InvalidOperationException>(() => generator.Generate(duplicateDescriptors));
        Assert.Contains("Duplicate artifact ID detected", exception.Message);
    }

    [Fact]
    public void ArtifactGenerator_ValidatesDuplicateFileNames()
    {
        var desc1 = new GeneratedArtifactDescriptor("id1", "file.json", "config/file.json", "application/json", "cap1", "OBS-001", "OBS-101", "desc", "gen", "ev.1", "{}");
        var desc2 = new GeneratedArtifactDescriptor("id2", "file.json", "other/file.json", "application/json", "cap2", "OBS-002", "OBS-102", "desc", "gen", "ev.2", "{}");

        var generator = new SaaSFoundry.Plugins.Observability.Traceability.ArtifactGenerator();
        var diagnostics = generator.Validate(new[] { desc1, desc2 });
        Assert.Contains(diagnostics, d => d.Code == "ERR_DUPLICATE_FILENAME" && d.IsError);
    }

    [Fact]
    public void ArtifactGenerator_ValidatesCircularDependencies()
    {
        var descA = new GeneratedArtifactDescriptor("id.a", "a.json", "config/a.json", "application/json", "cap", "OBS-001", "OBS-101", "desc", "gen", "ev.a", "{}", Dependencies: new[] { "id.b" });
        var descB = new GeneratedArtifactDescriptor("id.b", "b.json", "config/b.json", "application/json", "cap", "OBS-001", "OBS-101", "desc", "gen", "ev.b", "{}", Dependencies: new[] { "id.a" });

        var generator = new SaaSFoundry.Plugins.Observability.Traceability.ArtifactGenerator();
        var diagnostics = generator.Validate(new[] { descA, descB });
        Assert.Contains(diagnostics, d => d.Code == "ERR_CIRCULAR_DEPENDENCY" && d.IsError);
    }

    [Fact]
    public void ArtifactGenerator_ValidatesMissingDependencies_WhenStrictClosureEnforced()
    {
        var desc = new GeneratedArtifactDescriptor("id.main", "main.json", "config/main.json", "application/json", "cap", "OBS-001", "OBS-101", "desc", "gen", "ev.main", "{}", Dependencies: new[] { "id.missing" });

        var generator = new SaaSFoundry.Plugins.Observability.Traceability.ArtifactGenerator();
        
        // Allowed when external dependencies are permitted
        var lenientDiagnostics = generator.Validate(new[] { desc }, allowExternalDependencies: true);
        Assert.DoesNotContain(lenientDiagnostics, d => d.IsError);

        // Rejected when evaluating strict closure
        var strictDiagnostics = generator.Validate(new[] { desc }, allowExternalDependencies: false);
        Assert.Contains(strictDiagnostics, d => d.Code == "ERR_MISSING_DEPENDENCY" && d.IsError);
    }

    [Fact]
    public void ArtifactGenerator_ValidatesInvalidCanonAndImplementationReferences()
    {
        var desc = new GeneratedArtifactDescriptor("id.invalid", "bad.json", "config/bad.json", "application/json", "cap", "INVALID-CANON", "BAD-IMPL", "desc", "gen", "ev.bad", "{}");

        var generator = new SaaSFoundry.Plugins.Observability.Traceability.ArtifactGenerator();
        var diagnostics = generator.Validate(new[] { desc });
        Assert.Contains(diagnostics, d => d.Code == "ERR_INVALID_CANON_REF" && d.IsError);
        Assert.Contains(diagnostics, d => d.Code == "ERR_INVALID_IMPL_REF" && d.IsError);
    }
}
