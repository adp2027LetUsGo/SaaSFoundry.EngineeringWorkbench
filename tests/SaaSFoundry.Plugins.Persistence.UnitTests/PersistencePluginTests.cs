using System.Linq;
using SaaSFoundry.Plugins.Persistence.Plugin;
using SaaSFoundry.SDK.Testing.Assertions;
using SaaSFoundry.SDK.Testing.Fixtures;
using SaaSFoundry.SDK.Plugins.Abstractions;
using Xunit;

namespace SaaSFoundry.Plugins.Persistence.UnitTests;

public class PersistencePluginTests : PluginTestFixture<PersistencePlugin>
{
    [Fact]
    public void PersistencePlugin_Identity_IsCanonical()
    {
        var plugin = CreatePlugin();
        Assert.Equal("persistence", plugin.Identity.PluginId);
        Assert.Equal("1.0.0", plugin.Identity.Version);
        Assert.Equal("SaaSFoundry Engineering", plugin.Identity.Author);
        Assert.StartsWith("SHA256:", plugin.Identity.Fingerprint);
    }

    [Fact]
    public void PersistencePlugin_Manifest_IsCanonical()
    {
        var plugin = CreatePlugin();
        ManifestAssertions.AssertExactMatch(plugin.Manifest, "persistence", "1.0.0");
        ManifestAssertions.AssertCompatible(plugin.Manifest, "net10.0");
        ManifestAssertions.AssertCompatible(plugin.Manifest, "NativeAOT");
    }

    [Fact]
    public void PersistencePlugin_Capabilities_AreRegistered()
    {
        var plugin = CreatePlugin();
        var connection = (ITraceablePluginCapability?)plugin.GetCapability("connection");
        Assert.NotNull(connection);
        CapabilityAssertions.AssertTraceable(connection);

        var query = (ITraceablePluginCapability?)plugin.GetCapability("query");
        Assert.NotNull(query);
        CapabilityAssertions.AssertTraceable(query);

        var jobStorage = (ITraceablePluginCapability?)plugin.GetCapability("jobstorage");
        Assert.NotNull(jobStorage);
        CapabilityAssertions.AssertTraceable(jobStorage);

        var idempotency = (ITraceablePluginCapability?)plugin.GetCapability("idempotency");
        Assert.NotNull(idempotency);
        CapabilityAssertions.AssertTraceable(idempotency);

        Assert.Equal(4, plugin.Capabilities.Count);
    }

    [Fact]
    public void ConnectionCapability_Governance_IsCanonical()
    {
        var plugin = CreatePlugin();
        var connection = (ITraceablePluginCapability?)plugin.GetCapability("connection");
        Assert.NotNull(connection);

        // We can cast to ITraceablePluginCapability if needed, but we rely on SDK Assertion methods if applicable
        Assert.Equal(SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance.RiskLevel.High, connection.GovernanceMetadata.Risk);
    }

    [Fact]
    public void QueryCapability_Governance_IsCanonical()
    {
        var plugin = CreatePlugin();
        var query = (ITraceablePluginCapability?)plugin.GetCapability("query");
        Assert.NotNull(query);

        Assert.Equal(SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance.RiskLevel.High, query.GovernanceMetadata.Risk);
    }
    [Fact]
    public void IdempotencyCapability_Governance_IsCanonical()
    {
        var plugin = CreatePlugin();
        var idempotency = (ITraceablePluginCapability?)plugin.GetCapability("idempotency");
        Assert.NotNull(idempotency);

        Assert.Equal(SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance.RiskLevel.High, idempotency.GovernanceMetadata.Risk);
    }
}
