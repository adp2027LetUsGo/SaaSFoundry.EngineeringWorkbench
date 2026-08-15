using System;
using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.SDK.Core.Diagnostics;
using SaaSFoundry.SDK.Core.Generators;
using Xunit;

namespace SaaSFoundry.SDK.Core.UnitTests;

public class GeneratorTests
{
    [Fact]
    public void Generator_EmptyInput_ReturnsEmptyManifest()
    {
        var generator = new SaaSFoundry.SDK.Core.Generators.ArtifactGenerator("test", "1.0", "1.0");
        var result = generator.Generate(Array.Empty<GeneratedArtifactDescriptor>());
        
        Assert.NotNull(result);
        Assert.Empty(result.GeneratedArtifacts);
        Assert.NotNull(result.Manifest);
        Assert.Empty(result.Manifest.Artifacts);
    }

    [Fact]
    public void Generator_SingleArtifact_ReturnsManifest()
    {
        var generator = new SaaSFoundry.SDK.Core.Generators.ArtifactGenerator("test", "1.0", "1.0");
        var desc = new GeneratedArtifactDescriptor("id1", "file.cs", "/", "text/plain", "cap1", "CANON-1", "IMPL-1", "Desc", "Gen", "Valid1", "content");
        
        var result = generator.Generate(new[] { desc });
        
        Assert.Single(result.GeneratedArtifacts);
        Assert.Equal(ArtifactCategory.SourceCode, result.GeneratedArtifacts[0].Category);
        Assert.NotNull(result.GeneratedArtifacts[0].Hash);
        Assert.StartsWith("SHA256:", result.GeneratedArtifacts[0].Hash);
        Assert.Single(result.TraceabilityRecords);
        Assert.Single(result.ValidationEvidence);
    }

    [Fact]
    public void Generator_DuplicateArtifactId_Throws()
    {
        var generator = new SaaSFoundry.SDK.Core.Generators.ArtifactGenerator("test", "1.0", "1.0");
        var desc1 = new GeneratedArtifactDescriptor("id1", "file1.cs", "/", "text/plain", "cap1", "CANON-1", "IMPL-1", "Desc", "Gen", "Valid1", "content");
        var desc2 = new GeneratedArtifactDescriptor("id1", "file2.cs", "/", "text/plain", "cap1", "CANON-1", "IMPL-1", "Desc", "Gen", "Valid1", "content");
        
        var ex = Assert.Throws<InvalidOperationException>(() => { generator.Generate(new[] { desc1, desc2 }); });
        Assert.Contains("ERR_DUPLICATE_ID", ex.Message);
    }

    [Fact]
    public void Generator_DuplicateFileName_Throws()
    {
        var generator = new SaaSFoundry.SDK.Core.Generators.ArtifactGenerator("test", "1.0", "1.0");
        var desc1 = new GeneratedArtifactDescriptor("id1", "file.cs", "/", "text/plain", "cap1", "CANON-1", "IMPL-1", "Desc", "Gen", "Valid1", "content");
        var desc2 = new GeneratedArtifactDescriptor("id2", "file.cs", "/", "text/plain", "cap1", "CANON-1", "IMPL-1", "Desc", "Gen", "Valid1", "content");
        
        var ex = Assert.Throws<InvalidOperationException>(() => { generator.Generate(new[] { desc1, desc2 }); });
        Assert.Contains("ERR_DUPLICATE_FILENAME", ex.Message);
    }

    [Fact]
    public void Generator_MissingDependencyStrict_Throws()
    {
        var generator = new SaaSFoundry.SDK.Core.Generators.ArtifactGenerator("test", "1.0", "1.0");
        var desc = new GeneratedArtifactDescriptor("id1", "file.cs", "/", "text/plain", "cap1", "CANON-1", "IMPL-1", "Desc", "Gen", "Valid1", "content", Dependencies: new[] { "id2" });
        
        var ex = Assert.Throws<InvalidOperationException>(() => { generator.Generate(new[] { desc }, allowExternalDependencies: false); });
        Assert.Contains("ERR_MISSING_DEPENDENCY", ex.Message);
    }

    [Fact]
    public void Generator_MissingDependencyExternal_Allowed()
    {
        var generator = new SaaSFoundry.SDK.Core.Generators.ArtifactGenerator("test", "1.0", "1.0");
        var desc = new GeneratedArtifactDescriptor("id1", "file.cs", "/", "text/plain", "cap1", "CANON-1", "IMPL-1", "Desc", "Gen", "Valid1", "content", Dependencies: new[] { "id2" });
        
        var result = generator.Generate(new[] { desc }, allowExternalDependencies: true);
        Assert.Single(result.GeneratedArtifacts);
    }

    [Fact]
    public void Generator_CircularDependency_Throws()
    {
        var generator = new SaaSFoundry.SDK.Core.Generators.ArtifactGenerator("test", "1.0", "1.0");
        var desc1 = new GeneratedArtifactDescriptor("id1", "file1.cs", "/", "text/plain", "cap1", "CANON-1", "IMPL-1", "Desc", "Gen", "Valid1", "content", Dependencies: new[] { "id2" });
        var desc2 = new GeneratedArtifactDescriptor("id2", "file2.cs", "/", "text/plain", "cap1", "CANON-1", "IMPL-1", "Desc", "Gen", "Valid1", "content", Dependencies: new[] { "id1" });
        
        var ex = Assert.Throws<InvalidOperationException>(() => { generator.Generate(new[] { desc1, desc2 }); });
        Assert.Contains("ERR_CIRCULAR_DEPENDENCY", ex.Message);
    }
}
