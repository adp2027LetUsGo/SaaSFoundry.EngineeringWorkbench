using SaaSFoundry.SDK.Core.Generators;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;
using SaaSFoundry.Plugins.Observability.Capabilities.Logging;
using SaaSFoundry.Plugins.Observability.Capabilities.Validation;
using SaaSFoundry.Plugins.Observability.Traceability;
using Xunit;

namespace SaaSFoundry.Plugins.Observability.UnitTests;

public sealed class ValidationCapabilityTests
{
    [Fact]
    public void ValidationCapability_GeneratesComplianceArtifacts_WithCorrectDependencies()
    {
        var capability = new ValidationCapability();
        Assert.Equal("validation", capability.Id);

        var descriptors = capability.GetArtifactDescriptors();
        Assert.Equal(3, descriptors.Count);

        Assert.All(descriptors, d =>
        {
            Assert.Equal("OBS-010", d.CanonReference);
            Assert.Equal("OBS-110", d.ImplementationReference);
            Assert.Equal("validation", d.CapabilityId);
            Assert.False(string.IsNullOrWhiteSpace(d.Content));
        });

        // 1. Validation Engine Source
        var engine = descriptors.Single(d => d.ArtifactId == "obs.validation.engine");
        Assert.Equal("ObservabilityValidationEngine.cs", engine.FileName);
        Assert.Equal(ArtifactCategory.SourceCode, engine.Category);
        Assert.NotNull(engine.Dependencies);
        Assert.Contains("obs.configuration.appsettings", engine.Dependencies!);
        Assert.Contains("obs.documentation.traceability.matrix", engine.Dependencies!);

        // 2. Validation Policy
        var policy = descriptors.Single(d => d.ArtifactId == "obs.validation.policy");
        Assert.Equal("validation-policy.json", policy.FileName);
        Assert.Equal(ArtifactCategory.Configuration, policy.Category);
        Assert.NotNull(policy.Dependencies);
        Assert.Contains("obs.validation.engine", policy.Dependencies!);
        Assert.Contains("RULE-HASH-001", policy.Content);

        // 3. Validation Compliance Summary
        var summary = descriptors.Single(d => d.ArtifactId == "obs.validation.summary");
        Assert.Equal("Validation-Compliance-Summary.json", summary.FileName);
        Assert.Equal(ArtifactCategory.Evidence, summary.Category);
        Assert.NotNull(summary.Dependencies);
        Assert.Contains("obs.validation.engine", summary.Dependencies!);
        Assert.Contains("obs.validation.policy", summary.Dependencies!);
    }

    [Fact]
    public void VerifyCompliance_ValidatesPackageRules()
    {
        var capability = new ValidationCapability();
        var logging = new LoggingCapability();
        
        var generator = new SaaSFoundry.Plugins.Observability.Traceability.ArtifactGenerator();
        var result = generator.Generate(logging.GetArtifactDescriptors(), allowExternalDependencies: true);

        var builder = new SaaSFoundry.SDK.Packaging.Builders.PackageBuilder("test-plugin", "1.0.0", "1.0.0");
        var validPackage = builder.Build("pkg-obs-logging", "Logging test pkg", result).Package!;

        // 1. Valid package passes compliance clean
        var validDiagnostics = capability.VerifyCompliance(validPackage);
        Assert.DoesNotContain(validDiagnostics, d => d.IsError);

        // 2. Null package fails
        var nullDiagnostics = capability.VerifyCompliance(null);
        Assert.Contains(nullDiagnostics, d => d.Code == "ERR_NULL_PACKAGE" && d.IsError);

        // 3. Manipulated package hash fails
        var corruptHashPackage = validPackage with { PackageHash = "INVALID_HASH" };
        var hashDiagnostics = capability.VerifyCompliance(corruptHashPackage);
        Assert.Contains(hashDiagnostics, d => d.Code == "ERR_MISSING_HASH" && d.IsError);
    }

    [Fact]
    public void ValidationCapability_GovernanceMetadata_IsHighRisk_WithCompliancePermission()
    {
        var capability = new ValidationCapability();
        var metadata = capability.GovernanceMetadata;

        Assert.NotNull(metadata);
        Assert.Equal("validation.generate", metadata.OperationType);
        Assert.Equal(RiskLevel.High, metadata.Risk);
        Assert.Contains("ExecuteComplianceValidation", metadata.RequiredPermissions);
        Assert.Contains("Package integrity hash exists", metadata.ValidationRequirements);
        Assert.Contains("All artifacts have traceability records", metadata.ValidationRequirements);
    }
}
