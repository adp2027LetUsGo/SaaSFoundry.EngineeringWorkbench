using System.Threading.Tasks;
using SaaSFoundry.Agents.Reference;
using SaaSFoundry.EngineeringWorkbench.AgentRuntime.Events;
using SaaSFoundry.EngineeringWorkbench.AgentRuntime.Lifecycle;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;
using Xunit;

namespace SaaSFoundry.EngineeringWorkbench.AgentRuntime.UnitTests;

public sealed class AgentEventTests
{
    [Fact]
    public async Task EventBus_PublishAndRetrieveAuditHistory_Succeeds()
    {
        var bus = new DefaultAgentEventBus();
        var manager = new AgentLifecycleManager("agent-alpha", bus);
        var agent = new ObservabilityAgent();

        await manager.RegisterAgentAsync(agent);
        await manager.LoadAgentAsync();
        await manager.ValidateAgentAsync();
        await manager.ActivateAgentAsync();

        var history = bus.GetAuditHistory();
        Assert.NotEmpty(history);
        Assert.Contains(history, e => e is AgentRegisteredEvent);
        Assert.Contains(history, e => e is AgentActivatedEvent);
        Assert.Contains(history, e => e is AgentStateTransitionEvent trans && trans.NewState == AgentLifecycleState.Active);

        var alphaEvents = bus.GetEventsForAgent("agent-alpha");
        Assert.Equal(history.Count, alphaEvents.Count);
    }
}
