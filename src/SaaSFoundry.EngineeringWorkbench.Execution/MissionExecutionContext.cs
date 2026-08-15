#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using SaaSFoundry.EngineeringWorkbench.AgentGovernance.Engine;
using SaaSFoundry.EngineeringWorkbench.AgentRuntime.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Missions;
using SaaSFoundry.EngineeringWorkbench.Execution.Blackboard;

namespace SaaSFoundry.EngineeringWorkbench.Execution;

/// <summary>
/// Provides a deterministic or system wall-clock timer for precise execution measurement and replay.
/// </summary>
public sealed class ExecutionClock
{
    private long _fixedTimestampMilliseconds;
    private readonly bool _isDeterministic;

    /// <summary>Gets a value indicating whether this clock runs in deterministic simulation mode.</summary>
    public bool IsDeterministic => _isDeterministic;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionClock"/> class.
    /// </summary>
    /// <param name="isDeterministic">True to use fixed internal time advancement; false for system wall-clock.</param>
    /// <param name="fixedTimestampMilliseconds">Initial epoch timestamp for deterministic execution.</param>
    public ExecutionClock(bool isDeterministic = false, long fixedTimestampMilliseconds = 0)
    {
        _isDeterministic = isDeterministic;
        _fixedTimestampMilliseconds = fixedTimestampMilliseconds > 0 ? fixedTimestampMilliseconds : 1700000000000L;
    }

    /// <summary>
    /// Returns the current execution epoch timestamp in milliseconds.
    /// </summary>
    public long GetCurrentTimestampMilliseconds()
    {
        if (_isDeterministic)
        {
            return _fixedTimestampMilliseconds;
        }
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Manually advances the internal deterministic clock by the specified duration in milliseconds.
    /// </summary>
    public void AdvanceMilliseconds(long milliseconds)
    {
        if (_isDeterministic)
        {
            _fixedTimestampMilliseconds += milliseconds;
        }
    }
}

/// <summary>
/// Encapsulates the governance verification policy engine and enforcement posture for a mission.
/// </summary>
public sealed class GovernanceContext
{
    /// <summary>Gets the evaluation engine responsible for policy checks.</summary>
    public AgentGovernanceEngine GovernanceEngine { get; }
    /// <summary>Gets a value indicating whether unapproved actions result in hard blocking.</summary>
    public bool StrictEnforcement { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GovernanceContext"/> class.
    /// </summary>
    /// <param name="governanceEngine">The configured governance engine.</param>
    /// <param name="strictEnforcement">Whether strict enforcement is enabled.</param>
    public GovernanceContext(AgentGovernanceEngine governanceEngine, bool strictEnforcement = true)
    {
        GovernanceEngine = governanceEngine;
        StrictEnforcement = strictEnforcement;
    }

    /// <summary>Gets the standard secure default governance runtime configuration.</summary>
    public static GovernanceContext Default => new(new AgentGovernanceEngine(), true);
}

/// <summary>
/// Represents the comprehensive operational state and shared environment for an executing mission.
/// </summary>
/// <param name="MissionIdentity">The immutable mission identity record.</param>
/// <param name="MissionMetadata">Associated operational metadata.</param>
/// <param name="MissionContext">Initial invocation parameter context.</param>
/// <param name="MissionExecutionOptions">Configured concurrency and resilience scheduling controls.</param>
/// <param name="AgentRegistry">Registry of available governed agent identities and capabilities.</param>
/// <param name="GovernanceContext">Active governance evaluation engine and security posture.</param>
/// <param name="MissionBlackboard">Canonical shared communication surface for agent data exchange.</param>
/// <param name="ExecutionClock">Time measurement service governing timestamps and deterministic replay.</param>
/// <param name="CorrelationId">Unique trace execution run identifier.</param>
/// <param name="ParentMissionId">Optional parent mission identifier if executing as a sub-mission.</param>
/// <param name="CancellationToken">Token for signaling execution cancellation across workers.</param>
/// <param name="ExecutionVariables">Immutable dictionary of base configuration variables.</param>
/// <param name="SharedMissionState">Shared cross-node environmental properties.</param>
public sealed record MissionExecutionContext(
    MissionIdentity MissionIdentity,
    MissionMetadata MissionMetadata,
    MissionContext MissionContext,
    MissionExecutionOptions MissionExecutionOptions,
    AgentRegistry AgentRegistry,
    GovernanceContext GovernanceContext,
    MissionBlackboard MissionBlackboard,
    ExecutionClock ExecutionClock,
    string CorrelationId,
    string? ParentMissionId,
    CancellationToken CancellationToken,
    IReadOnlyDictionary<string, string> ExecutionVariables,
    IReadOnlyDictionary<string, string> SharedMissionState
)
{
    /// <summary>
    /// Creates a new operational execution context initialized from mission definitions and agent registry.
    /// </summary>
    /// <param name="context">Initial invocation parameter context.</param>
    /// <param name="metadata">Associated mission metadata.</param>
    /// <param name="registry">Registry of available governed agents.</param>
    /// <param name="governanceContext">Optional custom governance posture.</param>
    /// <param name="isDeterministicClock">True to force deterministic simulated time; false for live system clock.</param>
    public static MissionExecutionContext Create(
        MissionContext context,
        MissionMetadata metadata,
        AgentRegistry registry,
        GovernanceContext? governanceContext = null,
        bool isDeterministicClock = false)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (metadata == null) throw new ArgumentNullException(nameof(metadata));
        if (registry == null) throw new ArgumentNullException(nameof(registry));

        var clock = new ExecutionClock(isDeterministicClock, isDeterministicClock ? context.MissionIdentity.CreationTimestamp : 0);
        var blackboard = new MissionBlackboard();
        foreach (var kvp in context.Configuration)
        {
            blackboard.SetVariable(kvp.Key, kvp.Value);
        }

        return new MissionExecutionContext(
            context.MissionIdentity,
            metadata,
            context,
            context.ExecutionOptions,
            registry,
            governanceContext ?? GovernanceContext.Default,
            blackboard,
            clock,
            isDeterministicClock ? context.MissionIdentity.MissionFingerprint : Guid.NewGuid().ToString("N"),
            null,
            CancellationToken.None,
            new Dictionary<string, string>(context.Configuration, StringComparer.Ordinal),
            new Dictionary<string, string>(StringComparer.Ordinal)
        );
    }
}
