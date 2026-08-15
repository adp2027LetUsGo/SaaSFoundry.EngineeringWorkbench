#nullable enable

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Missions;
using SaaSFoundry.EngineeringWorkbench.Execution.Blackboard;

namespace SaaSFoundry.EngineeringWorkbench.Execution.Hooks;

/// <summary>
/// Contract for persistence and recovery of mission execution blackboard snapshots.
/// </summary>
public interface IStateManagementProvider
{
    /// <summary>Persists an immutable blackboard snapshot for a given mission ID.</summary>
    Task SaveStateAsync(string missionId, MissionBlackboardSnapshot snapshot, CancellationToken cancellationToken = default);
    /// <summary>Retrieves a previously persisted blackboard snapshot if present.</summary>
    Task<MissionBlackboardSnapshot?> LoadStateAsync(string missionId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contract for cross-session long-term memory storage and semantic recall for governed agents.
/// </summary>
public interface ILongTermMemoryProvider
{
    /// <summary>Stores long-term agent state under an indexed key.</summary>
    Task StoreMemoryAsync(string agentId, string key, string data, CancellationToken cancellationToken = default);
    /// <summary>Retrieves agent state associated with the given key.</summary>
    Task<string?> RetrieveMemoryAsync(string agentId, string key, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contract for recording and querying historical engineering patterns across project repositories.
/// </summary>
public interface IEngineeringMemoryProvider
{
    /// <summary>Records an engineering solution pattern with appropriate metadata description.</summary>
    Task RecordEngineeringPatternAsync(string patternId, string description, CancellationToken cancellationToken = default);
    /// <summary>Queries existing engineering solution patterns matching semantic search criteria.</summary>
    Task<IReadOnlyList<string>> QueryPatternsAsync(string query, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contract for metrics collection and performance evaluation across execution runs.
/// </summary>
public interface IBenchmarkEngine
{
    /// <summary>Records an empirical execution performance metric for a mission.</summary>
    Task RecordMetricsAsync(string missionId, string metricName, double value, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contract for deterministic playback and simulation of recorded mission timelines.
/// </summary>
public interface IReplayEngine
{
    /// <summary>Re-executes an archived mission deterministically from recorded inputs.</summary>
    Task<MissionResult> ReplayMissionAsync(string missionId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contract for pausing DAG execution to solicit human user feedback or governance authorization.
/// </summary>
public interface IHumanApprovalProvider
{
    /// <summary>Requests human authorization for a high-impact mission action.</summary>
    Task<bool> RequestHumanApprovalAsync(string missionId, string actionDescription, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contract for Retrieval-Augmented Generation (RAG) querying against domain technical documentation.
/// </summary>
public interface IRagKnowledgeProvider
{
    /// <summary>Performs semantic query search across indexed documentation and codebases.</summary>
    Task<IReadOnlyList<string>> SearchKnowledgeBaseAsync(string query, int maxResults = 5, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contract for dispatching mission capability execution nodes to remote computation clusters.
/// </summary>
public interface IDistributedScheduler
{
    /// <summary>Dispatches a capability node to a specified remote worker node.</summary>
    Task<bool> DispatchToNodeAsync(string remoteNodeId, string capabilityId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contract for orchestrating decentralized agent swarms across shared operational goals.
/// </summary>
public interface ISwarmCoordinator
{
    /// <summary>Coordinates collaborative problem-solving across a registered collective of agent IDs.</summary>
    Task CoordinateSwarmAsync(string swarmId, IReadOnlyList<string> agentIds, CancellationToken cancellationToken = default);
}
