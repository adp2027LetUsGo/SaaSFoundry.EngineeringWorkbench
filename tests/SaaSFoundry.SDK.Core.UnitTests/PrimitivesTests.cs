using System;
using SaaSFoundry.SDK.Core.Primitives;
using Xunit;

namespace SaaSFoundry.SDK.Core.UnitTests;

public class PrimitivesTests
{
    [Fact]
    public void PluginId_Equality_Works()
    {
        var id1 = new PluginId("auth");
        var id2 = new PluginId("auth");
        var id3 = new PluginId("db");

        Assert.Equal(id1, id2);
        Assert.NotEqual(id1, id3);
        Assert.True(id1 == id2);
        Assert.False(id1 == id3);
    }

    [Fact]
    public void CapabilityId_Equality_Works()
    {
        var id1 = new CapabilityId("cap1");
        var id2 = new CapabilityId("cap1");

        Assert.Equal(id1, id2);
        Assert.Equal("cap1", id1.Value);
    }

    [Fact]
    public void PackageId_Equality_Works()
    {
        var id1 = new PackageId("pkg1");
        var id2 = new PackageId("pkg1");

        Assert.Equal(id1, id2);
    }

    [Fact]
    public void ArtifactId_Equality_Works()
    {
        var id1 = new ArtifactId("art1");
        var id2 = new ArtifactId("art1");

        Assert.Equal(id1, id2);
    }

    [Fact]
    public void ExtensionPointId_Equality_Works()
    {
        var id1 = new ExtensionPointId("ext1");
        var id2 = new ExtensionPointId("ext1");

        Assert.Equal(id1, id2);
    }
}
