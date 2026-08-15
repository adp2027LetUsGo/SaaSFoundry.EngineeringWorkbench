using SaaSFoundry.SDK.Core.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation;
using SaaSFoundry.Plugins.Observability.Capabilities.Logging;
using SaaSFoundry.Plugins.Observability.Traceability;
using Xunit;

namespace SaaSFoundry.Plugins.Observability.UnitTests;

public sealed class EngineeringPackageValidationTests
{
    private static ArtifactGenerationResult GetValidLoggingResult()
    {
        var logging = new LoggingCapability();
        var generator = new SaaSFoundry.Plugins.Observability.Traceability.ArtifactGenerator();
        return generator.Generate(logging.GetArtifactDescriptors());
    }

    [Fact]
    public void Validate_DetectsNullResult()
    {
        var builder = new SaaSFoundry.SDK.Packaging.Builders.PackageBuilder("test-plugin", "1.0.0", "1.0.0");
        var diagnostics = builder.Validate(null);
        Assert.Contains(diagnostics, d => d.Code == "ERR_NULL_RESULT" && d.IsError);
    }

    [Fact]
    public void Validate_DetectsMissingManifest()
    {
        var valid = GetValidLoggingResult();
        var invalidResult = new ArtifactGenerationResult(
            valid.GeneratedArtifacts,
            null!, // Missing manifest
            valid.TraceabilityRecords,
            valid.ValidationEvidence,
            valid.ExecutionSummary
        );

        var builder = new SaaSFoundry.SDK.Packaging.Builders.PackageBuilder("test-plugin", "1.0.0", "1.0.0");
        var diagnostics = builder.Validate(invalidResult);
        Assert.Contains(diagnostics, d => d.Code == "ERR_MISSING_MANIFEST" && d.IsError);
        var prepResult = builder.Build("pkg-bad", "Desc", invalidResult);
        Assert.False(prepResult.IsSuccess);
    }

    [Fact]
    public void Validate_DetectsEmptyArtifacts()
    {
        var valid = GetValidLoggingResult();
        var emptyResult = new ArtifactGenerationResult(
            Array.Empty<GeneratedArtifactDescriptor>(),
            valid.Manifest,
            valid.TraceabilityRecords,
            valid.ValidationEvidence,
            valid.ExecutionSummary
        );

        var builder = new SaaSFoundry.SDK.Packaging.Builders.PackageBuilder("test-plugin", "1.0.0", "1.0.0");
        var diagnostics = builder.Validate(emptyResult);
        Assert.Contains(diagnostics, d => d.Code == "ERR_EMPTY_ARTIFACTS" && d.IsError);
    }

    [Fact]
    public void Validate_DetectsDuplicateArtifactIds()
    {
        var valid = GetValidLoggingResult();
        var duplicates = valid.GeneratedArtifacts.Concat(new[] { valid.GeneratedArtifacts[0] }).ToList();

        var duplicateResult = new ArtifactGenerationResult(
            duplicates,
            valid.Manifest,
            valid.TraceabilityRecords,
            valid.ValidationEvidence,
            valid.ExecutionSummary
        );

        var builder = new SaaSFoundry.SDK.Packaging.Builders.PackageBuilder("test-plugin", "1.0.0", "1.0.0");
        var diagnostics = builder.Validate(duplicateResult);
        Assert.Contains(diagnostics, d => d.Code == "ERR_DUPLICATE_ARTIFACT_ID" && d.IsError);
        Assert.Contains(diagnostics, d => d.ArtifactId == valid.GeneratedArtifacts[0].ArtifactId);
    }

    [Fact]
    public void Validate_DetectsMissingOrMismatchedEvidence()
    {
        var valid = GetValidLoggingResult();

        // Case 1: Empty evidence
        var emptyEvidenceResult = new ArtifactGenerationResult(
            valid.GeneratedArtifacts,
            valid.Manifest,
            valid.TraceabilityRecords,
            Array.Empty<SaaSFoundry.SDK.Core.Diagnostics.ValidationEvidence>(),
            valid.ExecutionSummary
        );

        var builder = new SaaSFoundry.SDK.Packaging.Builders.PackageBuilder("test-plugin", "1.0.0", "1.0.0");
        var diagnosticsEmpty = builder.Validate(emptyEvidenceResult);
        Assert.Contains(diagnosticsEmpty, d => d.Code == "ERR_MISSING_EVIDENCE" && d.IsError);

        // Case 2: Mismatched count
        var mismatchedEvidenceResult = new ArtifactGenerationResult(
            valid.GeneratedArtifacts,
            valid.Manifest,
            valid.TraceabilityRecords,
            valid.ValidationEvidence.Take(2).ToList(),
            valid.ExecutionSummary
        );
        var diagnosticsMismatched = builder.Validate(mismatchedEvidenceResult);
        Assert.Contains(diagnosticsMismatched, d => d.Code == "ERR_MISSING_EVIDENCE" && d.IsError);
    }

    [Fact]
    public void Validate_DetectsInvalidDependencyGraph()
    {
        var valid = GetValidLoggingResult();
        var strippedManifest = new ArtifactManifest(
            valid.Manifest.PluginId,
            valid.Manifest.PluginVersion,
            valid.Manifest.GenerationTime,
            valid.Manifest.GeneratorVersion,
            valid.Manifest.Artifacts,
            valid.Manifest.TraceabilityRecords,
            valid.Manifest.ValidationEvidence,
            null // Missing dependency graph
        );

        var invalidResult = new ArtifactGenerationResult(
            valid.GeneratedArtifacts,
            strippedManifest,
            valid.TraceabilityRecords,
            valid.ValidationEvidence,
            valid.ExecutionSummary
        );

        var builder = new SaaSFoundry.SDK.Packaging.Builders.PackageBuilder("test-plugin", "1.0.0", "1.0.0");
        var diagnostics = builder.Validate(invalidResult);
        Assert.Contains(diagnostics, d => d.Code == "ERR_INVALID_DEPENDENCY_GRAPH" && d.IsError);
    }

    [Fact]
    public void Validate_DetectsIncompleteTraceability()
    {
        var valid = GetValidLoggingResult();
        var incompleteRecords = valid.TraceabilityRecords.Take(1).ToList();

        var invalidResult = new ArtifactGenerationResult(
            valid.GeneratedArtifacts,
            valid.Manifest,
            incompleteRecords,
            valid.ValidationEvidence,
            valid.ExecutionSummary
        );

        var builder = new SaaSFoundry.SDK.Packaging.Builders.PackageBuilder("test-plugin", "1.0.0", "1.0.0");
        var diagnostics = builder.Validate(invalidResult);
        Assert.Contains(diagnostics, d => d.Code == "ERR_INCOMPLETE_TRACEABILITY" && d.IsError);
    }
}
