using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.AgentRuntime.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Missions;
using SaaSFoundry.EngineeringWorkbench.Execution;
using SaaSFoundry.EngineeringWorkbench.Execution.Blackboard;
using SaaSFoundry.EngineeringWorkbench.Execution.Capabilities;
using SaaSFoundry.EngineeringWorkbench.Execution.Hooks;
using SaaSFoundry.EngineeringWorkbench.Execution.Resources;
using SaaSFoundry.EngineeringWorkbench.Execution.Strategies;

namespace SaaSFoundry.EngineeringWorkbench.Execution.UnitTests;

public class MissionExecutionTests
{
    private sealed class InMemoryStateProvider : IStateManagementProvider
    {
        private readonly Dictionary<string, MissionBlackboardSnapshot> _store = new();
        public Task SaveStateAsync(string missionId, MissionBlackboardSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            _store[missionId] = snapshot;
            return Task.CompletedTask;
        }
        public Task<MissionBlackboardSnapshot?> LoadStateAsync(string missionId, CancellationToken cancellationToken = default)
        {
            _store.TryGetValue(missionId, out var snapshot);
            return Task.FromResult(snapshot);
        }
    }

    [Fact]
    public void Blackboard_SynchronizedModifications_AreReflectedInSnapshot()
    {
        var blackboard = new MissionBlackboard();
        blackboard.AddArtifact("artifact-1.json");
        blackboard.AddGeneratedArtifact("generated-v1.log");
        blackboard.AddDiagnostic("Warning: Check CPU usage.");
        blackboard.SetVariable("ENV", "Production");
        blackboard.AddHistoryEntry("Step 1 started.");

        var snapshot = blackboard.CreateSnapshot();
        Assert.Contains("artifact-1.json", snapshot.Artifacts);
        Assert.Contains("generated-v1.log", snapshot.GeneratedArtifacts);
        Assert.Equal("Production", snapshot.ExecutionVariables["ENV"]);
        Assert.Single(snapshot.ExecutionHistory);
    }

    [Fact]
    public void Blackboard_ConcurrentModifications_ThreadSafeUnderParallelExecution()
    {
        var blackboard = new MissionBlackboard();
        Parallel.For(0, 100, i =>
        {
            blackboard.AddArtifact($"art-{i}.json");
            blackboard.AddGeneratedArtifact($"gen-{i}.log");
            blackboard.AddDiagnostic($"Diag-{i}");
            blackboard.AddHistoryEntry($"Event-{i}");
        });

        var snapshot = blackboard.CreateSnapshot();
        Assert.Equal(100, snapshot.Artifacts.Count);
        Assert.Equal(100, snapshot.GeneratedArtifacts.Count);
        Assert.Equal(100, snapshot.Diagnostics.Count);
        Assert.Equal(100, snapshot.ExecutionHistory.Count);
    }

    [Fact]
    public void ExecutionClock_DeterministicMode_AdvancesConsistently()
    {
        var clock = new ExecutionClock(isDeterministic: true, fixedTimestampMilliseconds: 1000000L);
        Assert.True(clock.IsDeterministic);
        Assert.Equal(1000000L, clock.GetCurrentTimestampMilliseconds());

        clock.AdvanceMilliseconds(500L);
        Assert.Equal(1000500L, clock.GetCurrentTimestampMilliseconds());
    }

    [Fact]
    public void ExecutionStrategies_ExposeExpectedConcurrencyProperties()
    {
        var seq = new SequentialExecutionStrategy();
        Assert.False(seq.AllowParallel);
        Assert.Equal(1, seq.MaxConcurrency);

        var par = new ParallelExecutionStrategy(8);
        Assert.True(par.AllowParallel);
        Assert.Equal(8, par.MaxConcurrency);
    }

    [Fact]
    public async Task StateManagementProvider_SaveAndLoadState_PersistsBlackboardSnapshot()
    {
        var provider = new InMemoryStateProvider();
        var blackboard = new MissionBlackboard();
        blackboard.AddArtifact("saved-state.json");
        var snapshot = blackboard.CreateSnapshot();

        await provider.SaveStateAsync("M-State", snapshot);
        var loaded = await provider.LoadStateAsync("M-State");

        Assert.NotNull(loaded);
        Assert.Contains("saved-state.json", loaded!.Artifacts);
    }

    [Fact]
    public void AgentCapabilityDescriptor_EncapsulatesExpectedMetadata()
    {
        var desc = new AgentCapabilityDescriptor(
            AgentId: "Agent-1",
            CapabilityId: "Cap-1",
            CapabilityName: "Code Analysis",
            RiskLevel: AgentRiskLevel.Low,
            EstimatedDurationMilliseconds: 500L,
            Dependencies: Array.Empty<string>(),
            RequiredPermissions: new[] { "read-fs" },
            ProducesArtifacts: new[] { "analysis.json" },
            ConsumesArtifacts: Array.Empty<string>(),
            SupportsRetry: true,
            SupportsParallelExecution: true,
            SupportsApproval: false,
            SupportsCheckpointing: true
        );
        Assert.Equal("Cap-1", desc.CapabilityId);
        Assert.Equal(500L, desc.EstimatedDurationMilliseconds);
        Assert.True(desc.SupportsRetry);
        Assert.True(desc.SupportsCheckpointing);
    }

    [Fact]
    public void MissionExecutionContext_Create_InitializesProperly()
    {
        var id = new MissionIdentity("M-101", "1.0", 1700000000000L, "hash");
        var opts = new MissionExecutionOptions(true, 4, false, 1, 30000L);
        var ctx = new MissionContext(id, new Dictionary<string, string> { ["A"] = "B" }, new Dictionary<string, string> { ["ConfigKey"] = "Val" }, new[] { "out.json" }, opts);
        var meta = new MissionMetadata("Test Mission", "Desc", "System", Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), MissionPriority.Normal);
        var registry = new AgentRegistry();

        var execContext = MissionExecutionContext.Create(ctx, meta, registry, isDeterministicClock: true);
        Assert.NotNull(execContext.MissionBlackboard);
        Assert.Equal("Val", execContext.MissionBlackboard.CreateSnapshot().ExecutionVariables["ConfigKey"]);
        Assert.Equal(1700000000000L, execContext.ExecutionClock.GetCurrentTimestampMilliseconds());
    }
}
