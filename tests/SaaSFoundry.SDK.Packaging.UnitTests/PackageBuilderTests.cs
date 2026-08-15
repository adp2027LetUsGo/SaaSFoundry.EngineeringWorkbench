using System;
using System.Collections.Generic;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation;
using SaaSFoundry.SDK.Core.Diagnostics;
using SaaSFoundry.SDK.Core.Generators;
using SaaSFoundry.SDK.Packaging.Builders;
using Xunit;

namespace SaaSFoundry.SDK.Packaging.UnitTests;

public class PackageBuilderTests
{
    private readonly PackageBuilder _builder;

    public PackageBuilderTests()
    {
        _builder = new PackageBuilder("test-plugin", "1.0.0", "1.0.0");
    }

    [Fact]
    public void Validate_NullResult_ReturnsError()
    {
        var diagnostics = _builder.Validate(null);
        Assert.Contains(diagnostics, d => d.Code == "ERR_NULL_RESULT");
    }

    [Fact]
    public void Build_ValidResult_ProducesHash()
    {
        var art1 = new GeneratedArtifactDescriptor("art1", "art1.txt", "test/art1.txt", "text/plain", "cap1", "canon1", "impl1", "desc1", "gen1", "ev1", "content1", null, ArtifactCategory.SourceCode, null);
        var art2 = new GeneratedArtifactDescriptor("art2", "art2.txt", "test/art2.txt", "text/plain", "cap2", "canon2", "impl2", "desc2", "gen2", "ev2", "content2", null, ArtifactCategory.SourceCode, null);

        var trc1 = new TraceabilityRecord("canon1", "impl1", "cap1", "art1", "file", "art1.txt", "ev1", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "1.0", "notes1", ArtifactCategory.SourceCode);
        var trc2 = new TraceabilityRecord("canon2", "impl2", "cap2", "art2", "file", "art2.txt", "ev2", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "1.0", "notes2", ArtifactCategory.SourceCode);

        var ev1 = new SaaSFoundry.SDK.Core.Diagnostics.ValidationEvidence("test-plugin", "cap1", "generation", true, "ok", DateTimeOffset.UtcNow);
        var ev2 = new SaaSFoundry.SDK.Core.Diagnostics.ValidationEvidence("test-plugin", "cap2", "generation", true, "ok", DateTimeOffset.UtcNow);

        var manifest = new ArtifactManifest("test-plugin", "1.0.0", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "1.0.0", 
            new List<GeneratedArtifactDescriptor> { art1, art2 },
            new List<TraceabilityRecord> { trc1, trc2 },
            new List<SaaSFoundry.SDK.Core.Diagnostics.ValidationEvidence> { ev1, ev2 },
            new ArtifactDependencyGraph(new List<ArtifactDependencyNode>
            {
                new ArtifactDependencyNode("art1", new List<string>()),
                new ArtifactDependencyNode("art2", new List<string>())
            }));

        var result = new ArtifactGenerationResult(
            new List<GeneratedArtifactDescriptor> { art1, art2 },
            manifest,
            new List<TraceabilityRecord> { trc1, trc2 },
            new List<SaaSFoundry.SDK.Core.Diagnostics.ValidationEvidence> { ev1, ev2 },
            "Success summary"
        );

        var prepResult = _builder.Build("test-pkg", "test package", result, 123456789);
        
        Assert.True(prepResult.IsSuccess);
        Assert.NotNull(prepResult.Package);
        Assert.Equal("test-pkg", prepResult.Package.PackageId);
        Assert.NotNull(prepResult.Package.PackageHash);
        Assert.StartsWith("SHA256:", prepResult.Package.PackageHash);
        Assert.Equal(2, prepResult.Package.Artifacts.Count);
    }
}
