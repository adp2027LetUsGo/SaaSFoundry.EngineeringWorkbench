using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SaaSFoundry.Agents.Reference;
using SaaSFoundry.EngineeringWorkbench.AgentRuntime.Events;
using SaaSFoundry.EngineeringWorkbench.AgentRuntime.Identity;
using SaaSFoundry.EngineeringWorkbench.AgentRuntime.Lifecycle;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;
using Xunit;

namespace SaaSFoundry.EngineeringWorkbench.AgentRuntime.IntegrationTests;

public sealed class ObservabilityAgentOrchestrationTests
{
    [Fact]
    public async Task ObservabilityAgent_FullLifecycleAndPluginOrchestration_Succeeds()
    {
        var bus = new DefaultAgentEventBus();
        var registry = new AgentRegistry();
        var agent = new ObservabilityAgent();

        // 1. Register agent in runtime registry
        registry.Register(agent);
        Assert.True(registry.TryGetAgent("observability-agent", out var registeredAgent));
        Assert.NotNull(registeredAgent);

        // 2. Initialize lifecycle manager & execute initialization sequence
        var manager = new AgentLifecycleManager("observability-agent", bus);
        Assert.True(await manager.RegisterAgentAsync(agent));
        Assert.True(await manager.LoadAgentAsync());
        Assert.True(await manager.ValidateAgentAsync());
        Assert.True(await manager.ActivateAgentAsync());
        Assert.Equal(AgentLifecycleState.Active, manager.CurrentState);

        // 3. Enter Planning and Execution states
        Assert.True(await manager.StartPlanningAsync());
        Assert.True(await manager.StartExecutionAsync("observability.generate"));

        // 4. Orchestrate Observability Plugin explicitly without reflection
        var execContext = new AgentExecutionContext(
            ExecutionId: "exec-test-001",
            AgentId: agent.Identity.AgentId,
            Goal: "Generate all canonical observability engineering artifacts.",
            StartedTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Parameters: new Dictionary<string, string>()
        );

        var result = await agent.ExecuteAsync(execContext);

        // 5. Assert successful orchestration and artifact generation
        Assert.Equal(AgentExecutionStatus.Succeeded, result.Status);
        Assert.Null(result.ErrorMessage);
        Assert.NotNull(result.GeneratedArtifacts);
        Assert.Equal(37, result.GeneratedArtifacts.Count);
        Assert.Contains("logging.json", result.GeneratedArtifacts);
        Assert.Contains("README-Logging.md", result.GeneratedArtifacts);

        // 6. Complete execution and verify audit history
        Assert.True(await manager.CompleteExecutionAsync("observability.generate", isSuccess: true));
        Assert.Equal(AgentLifecycleState.Completed, manager.CurrentState);

        var auditHistory = bus.GetEventsForAgent("observability-agent");
        Assert.Contains(auditHistory, e => e is AgentRegisteredEvent);
        Assert.Contains(auditHistory, e => e is AgentActivatedEvent);
        Assert.Contains(auditHistory, e => e is AgentPlanningStartedEvent);
        Assert.Contains(auditHistory, e => e is AgentExecutionStartedEvent);
        Assert.Contains(auditHistory, e => e is AgentExecutionCompletedEvent comp && comp.IsSuccess);
    }
}
