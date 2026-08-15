using System.Threading.Tasks;
using SaaSFoundry.Agents.Reference;
using SaaSFoundry.EngineeringWorkbench.AgentRuntime.Events;
using SaaSFoundry.EngineeringWorkbench.AgentRuntime.Lifecycle;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;
using Xunit;

namespace SaaSFoundry.EngineeringWorkbench.AgentRuntime.UnitTests;

public sealed class AgentLifecycleTests
{
    [Fact]
    public void LifecycleManager_InitialState_IsCreated()
    {
        var bus = new DefaultAgentEventBus();
        var manager = new AgentLifecycleManager("test-agent", bus);
        Assert.Equal(AgentLifecycleState.Created, manager.CurrentState);
    }

    [Fact]
    public async Task LifecycleManager_LawfulSetupAndExecutionSequence_Succeeds()
    {
        var bus = new DefaultAgentEventBus();
        var manager = new AgentLifecycleManager("test-agent", bus);
        var agent = new ObservabilityAgent();

        Assert.True(await manager.RegisterAgentAsync(agent));
        Assert.Equal(AgentLifecycleState.Registered, manager.CurrentState);

        Assert.True(await manager.LoadAgentAsync());
        Assert.Equal(AgentLifecycleState.Loaded, manager.CurrentState);

        Assert.True(await manager.ValidateAgentAsync());
        Assert.Equal(AgentLifecycleState.Validated, manager.CurrentState);

        Assert.True(await manager.ActivateAgentAsync());
        Assert.Equal(AgentLifecycleState.Active, manager.CurrentState);

        Assert.True(await manager.StartPlanningAsync());
        Assert.Equal(AgentLifecycleState.Planning, manager.CurrentState);

        Assert.True(await manager.StartExecutionAsync());
        Assert.Equal(AgentLifecycleState.Executing, manager.CurrentState);

        Assert.True(await manager.WaitForInputAsync());
        Assert.Equal(AgentLifecycleState.WaitingForInput, manager.CurrentState);

        Assert.True(await manager.CompleteExecutionAsync());
        Assert.Equal(AgentLifecycleState.Completed, manager.CurrentState);
    }

    [Fact]
    public async Task LifecycleManager_InvalidTransition_IsRejected()
    {
        var bus = new DefaultAgentEventBus();
        var manager = new AgentLifecycleManager("test-agent", bus);

        // Cannot jump directly from Created to Executing or Active
        Assert.False(manager.CanTransitionTo(AgentLifecycleState.Executing));
        Assert.False(await manager.ActivateAgentAsync());
        Assert.Equal(AgentLifecycleState.Created, manager.CurrentState);
    }
}
