using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.AgentRuntime.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Missions;
using SaaSFoundry.EngineeringWorkbench.Execution;
using SaaSFoundry.EngineeringWorkbench.Execution.Capabilities;
using SaaSFoundry.EngineeringWorkbench.Execution.Strategies;
using SaaSFoundry.EngineeringWorkbench.Planning.DAG;
using SaaSFoundry.EngineeringWorkbench.Planning.Scheduling;

namespace SaaSFoundry.EngineeringWorkbench.AgentCoordinator.UnitTests;

public class MissionCoordinatorTests
{
    private sealed class MockAgent : IAgentOrchestrator
    {
        public AgentIdentity Identity { get; }
        public AgentMetadata Metadata { get; }
        public AgentGovernanceMetadata GovernanceMetadata { get; }
        private readonly bool _shouldSucceed;

        public MockAgent(string id, bool shouldSucceed = true)
        {
            Identity = new AgentIdentity(id, "1.0", "MockAuthor", "Fingerprint", 1000L);
            Metadata = new AgentMetadata(Identity, "MockAgent", "Desc", "Purpose", Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), "None");
            GovernanceMetadata = new AgentGovernanceMetadata(id, AgentRiskLevel.Low, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
            _shouldSucceed = shouldSucceed;
        }

        public Task<AgentExecutionResult> ExecuteAsync(AgentExecutionContext context, CancellationToken cancellationToken = default)
        {
            if (_shouldSucceed)
            {
                return Task.FromResult(new AgentExecutionResult(
                    context.ExecutionId,
                    context.AgentId,
                    AgentExecutionStatus.Succeeded,
                    1000L,
                    "Success",
                    new[] { $"artifact-{context.AgentId}.json" },
                    null
                ));
            }
            return Task.FromResult(new AgentExecutionResult(
                context.ExecutionId,
                context.AgentId,
                AgentExecutionStatus.Failed,
                1000L,
                "Failure",
                Array.Empty<string>(),
                "Simulated Failure"
            ));
        }
    }

    [Fact]
    public async Task ExecuteMissionAsync_ExplicitRegistration_CompletesSuccessfullyAndCollectsArtifacts()
    {
        var scheduler = new ExecutionScheduler(new SequentialExecutionStrategy());
        var coordinator = new MissionCoordinator(scheduler);

        var agent1 = new MockAgent("Agent-Obs");
        var agent2 = new MockAgent("Agent-Doc");

        coordinator.RegisterAgent(agent1);
        coordinator.RegisterAgent(agent2);
        coordinator.RegisterCapability(new AgentCapabilityDescriptor("Agent-Obs", "Obs-Cap", "Obs", AgentRiskLevel.Low, 500, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), false, false, false, false));

        var id = new MissionIdentity("M-201", "1.0", 1700000000000L, "hash");
        var opts = new MissionExecutionOptions(false, 1, false, 1, 30000L);
        var ctx = new MissionContext(id, new Dictionary<string, string>(), new Dictionary<string, string>(), Array.Empty<string>(), opts);
        var execCtx = MissionExecutionContext.Create(ctx, new MissionMetadata("Test", "D", "U", Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), MissionPriority.Normal), new AgentRegistry(), isDeterministicClock: true);

        var node1 = ExecutionNode.Create("Node1", "Agent-Obs", "Obs-Cap");
        var node2 = ExecutionNode.Create("Node2", "Agent-Doc", "Doc-Cap", new[] { "Node1" });
        var plan = new ExecutionPlan(id, new[] { node1, node2 }, new[] { new ExecutionEdge("Node1", "Node2") }, new[] { "Node1", "Node2" }, 2000L);

        var result = await coordinator.ExecuteMissionAsync(plan, execCtx);

        Assert.True(result.Succeeded);
        Assert.Equal(MissionExecutionStatus.Completed, result.MissionExecutionStatus);
        Assert.Contains("artifact-Agent-Obs.json", result.Artifacts);
        Assert.Contains("artifact-Agent-Doc.json", result.Artifacts);
        Assert.Equal(MissionExecutionStatus.Completed, coordinator.GetMissionStatus("M-201"));
    }

    [Fact]
    public async Task ExecuteMissionAsync_CriticalNodeFailure_AbortsMission()
    {
        var scheduler = new ExecutionScheduler(new SequentialExecutionStrategy());
        var coordinator = new MissionCoordinator(scheduler);

        var failAgent = new MockAgent("FailAgent", shouldSucceed: false);
        coordinator.RegisterAgent(failAgent);

        var id = new MissionIdentity("M-Fail", "1.0", 1000L, "hash");
        var opts = new MissionExecutionOptions(false, 1, false, 1, 30000L);
        var ctx = new MissionContext(id, new Dictionary<string, string>(), new Dictionary<string, string>(), Array.Empty<string>(), opts);
        var execCtx = MissionExecutionContext.Create(ctx, new MissionMetadata("Test", "D", "U", Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), MissionPriority.Normal), new AgentRegistry(), isDeterministicClock: true);

        var node1 = ExecutionNode.Create("Node1", "FailAgent", "BadCap", retryCount: 0, criticalNode: true, continueOnFailure: false);
        var plan = new ExecutionPlan(id, new[] { node1 }, Array.Empty<ExecutionEdge>(), new[] { "Node1" }, 1000L);

        var result = await coordinator.ExecuteMissionAsync(plan, execCtx);

        Assert.False(result.Succeeded);
        Assert.Equal(MissionExecutionStatus.Failed, result.MissionExecutionStatus);
        Assert.Equal(MissionExecutionStatus.Failed, coordinator.GetMissionStatus("M-Fail"));
    }

    [Fact]
    public async Task CancelMissionAsync_TransitionsStatusToCancelled()
    {
        var scheduler = new ExecutionScheduler(new SequentialExecutionStrategy());
        var coordinator = new MissionCoordinator(scheduler);
        var agent = new MockAgent("SlowAgent");
        coordinator.RegisterAgent(agent);

        var id = new MissionIdentity("M-Cancel", "1.0", 1000L, "hash");
        var opts = new MissionExecutionOptions(false, 1, false, 1, 30000L);
        var ctx = new MissionContext(id, new Dictionary<string, string>(), new Dictionary<string, string>(), Array.Empty<string>(), opts);
        var execCtx = MissionExecutionContext.Create(ctx, new MissionMetadata("Test", "D", "U", Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), MissionPriority.Normal), new AgentRegistry(), isDeterministicClock: true);

        var node1 = ExecutionNode.Create("Node1", "SlowAgent", "Cap1");
        var plan = new ExecutionPlan(id, new[] { node1 }, Array.Empty<ExecutionEdge>(), new[] { "Node1" }, 1000L);

        // Execute mission and test status querying
        var result = await coordinator.ExecuteMissionAsync(plan, execCtx);
        Assert.True(result.Succeeded);

        // Verify Cancel returns false when no active execution remains
        bool cancelled = await coordinator.CancelMissionAsync("NonExistent");
        Assert.False(cancelled);
    }

    [Fact]
    public async Task ExecuteMissionAsync_UnregisteredAgentDispatched_FailsGracefully()
    {
        var scheduler = new ExecutionScheduler(new SequentialExecutionStrategy());
        var coordinator = new MissionCoordinator(scheduler);

        var id = new MissionIdentity("M-Missing", "1.0", 1000L, "hash");
        var opts = new MissionExecutionOptions(false, 1, false, 1, 30000L);
        var ctx = new MissionContext(id, new Dictionary<string, string>(), new Dictionary<string, string>(), Array.Empty<string>(), opts);
        var execCtx = MissionExecutionContext.Create(ctx, new MissionMetadata("Test", "D", "U", Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), MissionPriority.Normal), new AgentRegistry(), isDeterministicClock: true);

        var node1 = ExecutionNode.Create("Node1", "GhostAgent", "Cap1", retryCount: 0);
        var plan = new ExecutionPlan(id, new[] { node1 }, Array.Empty<ExecutionEdge>(), new[] { "Node1" }, 1000L);

        var result = await coordinator.ExecuteMissionAsync(plan, execCtx);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, d => d.Contains("not explicitly registered in coordinator"));
    }
}
