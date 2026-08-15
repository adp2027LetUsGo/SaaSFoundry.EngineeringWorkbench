using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SaaSFoundry.Agents.Reference;
using SaaSFoundry.EngineeringWorkbench.AgentGovernance.Engine;
using SaaSFoundry.EngineeringWorkbench.AgentGovernance.Pipeline;
using SaaSFoundry.EngineeringWorkbench.AgentGovernance.Providers;
using SaaSFoundry.EngineeringWorkbench.AgentRuntime.Events;
using SaaSFoundry.EngineeringWorkbench.AgentRuntime.Identity;
using SaaSFoundry.EngineeringWorkbench.AgentRuntime.Lifecycle;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent.Governance;
using Xunit;

namespace SaaSFoundry.EngineeringWorkbench.AgentRuntime.IntegrationTests;

public class GovernedObservabilityAgentOrchestrationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task ObservabilityAgent_GovernedPipelineExecution_GeneratesAuditEventsAndArtifacts()
    {
        // 1. Initialize Event Bus and Governance Pipeline with audit tracking
        var eventBus = new DefaultAgentEventBus();
        var governanceEngine = new AgentGovernanceEngine();
        var approvalProvider = new DefaultAgentApprovalProvider(autoApprove: true);
        var pipeline = new GovernedAgentExecutionPipeline(governanceEngine, approvalProvider, eventBus);

        // 2. Initialize Agent with governed pipeline and Lifecycle Manager
        var agent = new ObservabilityAgent(pipeline);
        var registry = new AgentRegistry();
        registry.Register(agent);

        var manager = new AgentLifecycleManager("observability-agent", eventBus);

        // 3. Register and progress lifecycle to Active
        Assert.True(await manager.RegisterAgentAsync(agent));
        Assert.True(await manager.LoadAgentAsync());
        Assert.True(await manager.ValidateAgentAsync());
        Assert.True(await manager.ActivateAgentAsync());
        Assert.True(await manager.StartExecutionAsync("observability.generate"));

        // 4. Execute capability through Governed Pipeline
        var context = new AgentExecutionContext(
            ExecutionId: "exec-governance-int-001",
            AgentId: agent.Identity.AgentId,
            Goal: "Produce canonical observability package with complete governance audit trail.",
            StartedTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Parameters: new Dictionary<string, string>()
        );

        var result = await agent.ExecuteAsync(context);

        // 5. Assert successful governed execution and 37 deterministic artifacts
        Assert.NotNull(result);
        Assert.Equal(AgentExecutionStatus.Succeeded, result.Status);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(37, result.GeneratedArtifacts.Count);
        Assert.Contains("logging.json", result.GeneratedArtifacts);
        Assert.Contains("README-Logging.md", result.GeneratedArtifacts);

        Assert.True(await manager.CompleteExecutionAsync("observability.generate", isSuccess: true));

        // 6. Verify event audit trail from EventBus
        var history = eventBus.GetEventsForAgent(agent.Identity.AgentId);
        Assert.NotEmpty(history);
        Assert.Contains(history, e => e is AgentExecutionStartedEvent);
        Assert.Contains(history, e => e is AgentExecutionCompletedEvent);
        Assert.Contains(history, e => e.AgentId == agent.Identity.AgentId);
    }
}
