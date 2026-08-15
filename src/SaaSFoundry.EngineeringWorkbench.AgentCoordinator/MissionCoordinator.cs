#nullable enable

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.AgentRuntime.Events;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Missions;
using SaaSFoundry.EngineeringWorkbench.Execution;
using SaaSFoundry.EngineeringWorkbench.Execution.Capabilities;
using SaaSFoundry.EngineeringWorkbench.Planning.DAG;
using SaaSFoundry.EngineeringWorkbench.Planning.Scheduling;

namespace SaaSFoundry.EngineeringWorkbench.AgentCoordinator;

/// <summary>
/// Implements deterministic multi-agent mission coordination and task dispatching without reflection or assembly scanning.
/// </summary>
public sealed class MissionCoordinator : IAgentCoordinator
{
    private readonly ConcurrentDictionary<string, IAgentOrchestrator> _agents = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, AgentCapabilityDescriptor> _capabilities = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, MissionExecutionStatus> _statuses = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellations = new(StringComparer.Ordinal);
    private readonly ExecutionScheduler _scheduler;
    private readonly IAgentEventBus? _eventBus;

    /// <summary>Gets the optional agent event bus configured for lifecycle notification publishing.</summary>
    public IAgentEventBus? EventBus => _eventBus;

    /// <summary>
    /// Initializes a new instance of the <see cref="MissionCoordinator"/> class using the provided scheduler and optional event bus.
    /// </summary>
    /// <param name="scheduler">The DAG concurrency execution scheduler.</param>
    /// <param name="eventBus">Optional event bus for publishing real-time telemetry events.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="scheduler"/> is null.</exception>
    public MissionCoordinator(ExecutionScheduler scheduler, IAgentEventBus? eventBus = null)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _eventBus = eventBus;
    }

    /// <summary>
    /// Explicitly registers an agent without dynamic runtime scanning or reflection.
    /// </summary>
    public void RegisterAgent(IAgentOrchestrator agent)
    {
        if (agent == null) throw new ArgumentNullException(nameof(agent));
        _agents[agent.Identity.AgentId] = agent;
    }

    /// <summary>
    /// Explicitly registers an agent capability descriptor for planning and coordination.
    /// </summary>
    public void RegisterCapability(AgentCapabilityDescriptor capability)
    {
        if (capability == null) throw new ArgumentNullException(nameof(capability));
        _capabilities[capability.CapabilityId] = capability;
    }

    /// <inheritdoc />
    public MissionExecutionStatus GetMissionStatus(string missionId)
    {
        if (string.IsNullOrEmpty(missionId)) return MissionExecutionStatus.Created;
        return _statuses.TryGetValue(missionId, out var status) ? status : MissionExecutionStatus.Created;
    }

    /// <inheritdoc />
    public Task<bool> CancelMissionAsync(string missionId)
    {
        if (!string.IsNullOrEmpty(missionId) && _cancellations.TryGetValue(missionId, out var cts))
        {
            _statuses[missionId] = MissionExecutionStatus.Cancelled;
            cts.Cancel();
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public async Task<MissionResult> ExecuteMissionAsync(ExecutionPlan plan, MissionExecutionContext context, CancellationToken cancellationToken = default)
    {
        if (plan == null) throw new ArgumentNullException(nameof(plan));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var missionId = context.MissionIdentity.MissionId;
        _statuses[missionId] = MissionExecutionStatus.Executing;
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, context.CancellationToken);
        _cancellations[missionId] = cts;

        context.MissionBlackboard.AddHistoryEntry($"[MissionCoordinator] Starting execution of mission '{missionId}' over {plan.Nodes.Count} nodes.");
        long startTime = context.ExecutionClock.GetCurrentTimestampMilliseconds();

        try
        {
            bool allSucceeded = await _scheduler.ScheduleAndExecuteAsync(
                plan,
                context,
                async (node, ct) => await ExecuteNodeAsync(node, context, ct),
                cts.Token
            );

            long endTime = context.ExecutionClock.GetCurrentTimestampMilliseconds();
            long duration = Math.Max(1L, endTime - startTime);

            var finalStatus = allSucceeded
                ? MissionExecutionStatus.Completed
                : (cts.IsCancellationRequested ? MissionExecutionStatus.Cancelled : MissionExecutionStatus.Failed);

            _statuses[missionId] = finalStatus;

            var snapshot = context.MissionBlackboard.CreateSnapshot();
            return new MissionResult(
                context.MissionIdentity,
                finalStatus,
                snapshot.GeneratedArtifacts,
                snapshot.Diagnostics,
                snapshot.ValidationEvidence,
                duration,
                allSucceeded
            );
        }
        finally
        {
            _cancellations.TryRemove(missionId, out _);
        }
    }

    private async Task<bool> ExecuteNodeAsync(ExecutionNode node, MissionExecutionContext context, CancellationToken token)
    {
        if (!_agents.TryGetValue(node.AgentId, out var agent))
        {
            context.MissionBlackboard.AddDiagnostic($"[MissionCoordinator] Agent '{node.AgentId}' is not explicitly registered in coordinator.");
            return false;
        }

        context.MissionBlackboard.AddHistoryEntry($"[MissionCoordinator] Dispatching node '{node.NodeId}' (Capability: {node.CapabilityId}) to Agent '{node.AgentId}'.");
        context.ExecutionClock.AdvanceMilliseconds(node.EstimatedDurationMilliseconds);

        var agentContext = new AgentExecutionContext(
            ExecutionId: $"node-exec-{node.NodeId}",
            AgentId: node.AgentId,
            Goal: $"Execute capability {node.CapabilityId} for mission {context.MissionIdentity.MissionId}",
            StartedTimestamp: context.ExecutionClock.GetCurrentTimestampMilliseconds(),
            Parameters: context.ExecutionVariables
        );

        var result = await agent.ExecuteAsync(agentContext, token);
        context.MissionBlackboard.AddAgentResult(result);

        if (result.Status == AgentExecutionStatus.Succeeded)
        {
            context.MissionBlackboard.AddHistoryEntry($"[MissionCoordinator] Node '{node.NodeId}' completed successfully, producing {result.GeneratedArtifacts.Count} artifacts.");
            return true;
        }

        context.MissionBlackboard.AddDiagnostic($"[MissionCoordinator] Node '{node.NodeId}' execution failed: {result.ErrorMessage}");
        return false;
    }
}
