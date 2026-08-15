using System;
using SaaSFoundry.Agents.Reference;
using SaaSFoundry.EngineeringWorkbench.AgentRuntime.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;
using Xunit;

namespace SaaSFoundry.EngineeringWorkbench.AgentRuntime.UnitTests;

public sealed class AgentIdentityTests
{
    [Fact]
    public void Identity_HasRequiredImmutableFields()
    {
        var identity = new AgentIdentity("test-agent", "1.0.0", "Author", "SHA256:1234", 10000L);
        Assert.Equal("test-agent", identity.AgentId);
        Assert.Equal("1.0.0", identity.Version);
        Assert.Equal("Author", identity.Author);
        Assert.Equal("SHA256:1234", identity.Fingerprint);
        Assert.Equal(10000L, identity.CreatedTimestamp);
    }

    [Fact]
    public void AgentRegistry_RegisterAndRetrieve_Successful()
    {
        var registry = new AgentRegistry();
        var agent = new ObservabilityAgent();

        registry.Register(agent);

        Assert.True(registry.TryGetAgent("observability-agent", out var retrieved));
        Assert.NotNull(retrieved);
        Assert.Equal("observability-agent", retrieved.Identity.AgentId);
        Assert.Single(registry.GetAllRegisteredAgents());
    }

    [Fact]
    public void AgentRegistry_DuplicateAgentId_ThrowsInvalidOperationException()
    {
        var registry = new AgentRegistry();
        var agent1 = new ObservabilityAgent();
        var agent2 = new ObservabilityAgent();

        registry.Register(agent1);
        Assert.Throws<InvalidOperationException>(() => registry.Register(agent2));
    }

    [Fact]
    public void AgentRegistry_NullAgent_ThrowsArgumentNullException()
    {
        var registry = new AgentRegistry();
        Assert.Throws<ArgumentNullException>(() => registry.Register(null!));
    }
}
