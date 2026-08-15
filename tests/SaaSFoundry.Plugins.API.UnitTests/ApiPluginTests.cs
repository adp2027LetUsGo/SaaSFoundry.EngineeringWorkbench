using System.Linq;
using SaaSFoundry.Plugins.API.Plugin;
using SaaSFoundry.SDK.Testing.Assertions;
using Xunit;

namespace SaaSFoundry.Plugins.API.UnitTests;

public class ApiPluginTests
{
    [Fact]
    public void ApiPlugin_Manifest_IsCanonical()
    {
        var plugin = new ApiPlugin();
        
        ManifestAssertions.AssertExactMatch(plugin.Manifest, "api", "1.0.0");
        Assert.Equal("SaaSFoundry API Engineering Plugin", plugin.Manifest.Name);
        Assert.Equal("Production API infrastructure plugin providing Minimal API endpoint capabilities without reflection.", plugin.Manifest.Description);
        Assert.Contains("net10.0", plugin.Manifest.Compatibility);
        Assert.Contains("NativeAOT", plugin.Manifest.Compatibility);
    }

    [Fact]
    public void ApiPlugin_Identity_IsStable()
    {
        var plugin = new ApiPlugin();
        
        Assert.Equal("api", plugin.Identity.PluginId);
        Assert.Equal("1.0.0", plugin.Identity.Version);
        Assert.Equal("SaaSFoundry Engineering", plugin.Identity.Author);
        Assert.Equal("SHA256:8F3A2B7C1D2E5F0A1B2C3D4E5F6A7B8C9D0E1F2A3B4C5D6E7F8A9B0C1D2E4G", plugin.Identity.Fingerprint);
    }

    [Fact]
    public void ApiPlugin_Capabilities_AreTraceable()
    {
        var plugin = new ApiPlugin();
        
        var healthCapability = plugin.GetCapability("health");
        Assert.NotNull(healthCapability);
        
        CapabilityAssertions.AssertTraceable(healthCapability);
    }

    [Fact]
    public void ApiPlugin_Capabilities_HaveCorrectGovernance()
    {
        var plugin = new ApiPlugin();
        
        var healthCapability = plugin.GetCapability("health");
        Assert.NotNull(healthCapability);
        
        GovernanceAssertions.AssertGoverned(healthCapability);
    }
}
