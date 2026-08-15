using SaaSFoundry.SDK.Core.Generators;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;
using SaaSFoundry.Plugins.Observability.Capabilities.Documentation;
using SaaSFoundry.Plugins.Observability.Traceability;
using Xunit;

namespace SaaSFoundry.Plugins.Observability.UnitTests;

public sealed class DocumentationCapabilityTests
{
    [Fact]
    public void DocumentationCapability_GeneratesCanonicalMapping_AndEvidence()
    {
        var capability = new DocumentationCapability();
        Assert.Equal("documentation", capability.Id);

        var descriptors = capability.GetArtifactDescriptors();
        Assert.Equal(3, descriptors.Count);

        Assert.All(descriptors, d =>
        {
            Assert.Equal("OBS-009", d.CanonReference);
            Assert.Equal("OBS-109", d.ImplementationReference);
            Assert.Equal("documentation", d.CapabilityId);
            Assert.False(string.IsNullOrWhiteSpace(d.Content));
        });

        // 1. Canonical Traceability Matrix
        var matrix = descriptors.Single(d => d.ArtifactId == "obs.documentation.traceability.matrix");
        Assert.Equal("OBSERVABILITY-CANON-TRACEABILITY-MATRIX.md", matrix.FileName);
        Assert.Equal(ArtifactCategory.Documentation, matrix.Category);
        Assert.Contains("OBS-001", matrix.Content);
        Assert.Contains("OBS-101", matrix.Content);
        Assert.Contains("OBS-010", matrix.Content);
        Assert.Contains("OBS-110", matrix.Content);
        Assert.NotNull(matrix.Dependencies);
        Assert.Contains("obs.configuration.appsettings", matrix.Dependencies!);
        Assert.Contains("obs.alerts.rules.prometheus", matrix.Dependencies!);

        // 2. Architecture Guide
        var guide = descriptors.Single(d => d.ArtifactId == "obs.documentation.architecture.guide");
        Assert.Equal("Observability-Architecture-Guide.md", guide.FileName);
        Assert.Equal(ArtifactCategory.Documentation, guide.Category);
        Assert.NotNull(guide.Dependencies);
        Assert.Contains("obs.documentation.traceability.matrix", guide.Dependencies!);

        // 3. Documentation Evidence
        var evidence = descriptors.Single(d => d.ArtifactId == "obs.documentation.evidence");
        Assert.Equal("Evidence-Documentation.json", evidence.FileName);
        Assert.Equal(ArtifactCategory.Evidence, evidence.Category);
        Assert.NotNull(evidence.Dependencies);
        Assert.Contains("obs.documentation.traceability.matrix", evidence.Dependencies!);
        Assert.Contains("obs.documentation.architecture.guide", evidence.Dependencies!);
    }

    [Fact]
    public void DocumentationCapability_GovernanceMetadata_IsValid()
    {
        var capability = new DocumentationCapability();
        var metadata = capability.GovernanceMetadata;

        Assert.NotNull(metadata);
        Assert.Equal("documentation.generate", metadata.OperationType);
        Assert.Equal(RiskLevel.Low, metadata.Risk);
        Assert.Contains("GenerateEngineeringDocumentation", metadata.RequiredPermissions);
    }
}
