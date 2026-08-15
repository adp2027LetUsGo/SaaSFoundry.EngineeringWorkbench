#nullable enable

using System;
using System.Collections.Generic;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Missions;
using SaaSFoundry.EngineeringWorkbench.Execution.Resources;

namespace SaaSFoundry.EngineeringWorkbench.Planning.DAG;

/// <summary>
/// Represents a directional dependency link between two execution nodes in a mission DAG.
/// </summary>
/// <param name="SourceNodeId">Identifier of the prerequisite node that must complete first.</param>
/// <param name="TargetNodeId">Identifier of the dependent node awaiting completion.</param>
public sealed record ExecutionEdge(string SourceNodeId, string TargetNodeId);

/// <summary>
/// Represents an atomic unit of execution within a multi-agent engineering mission DAG.
/// </summary>
/// <param name="NodeId">Unique identifier of this step within the DAG.</param>
/// <param name="AgentId">Target governed agent identity assigned to execute this step.</param>
/// <param name="CapabilityId">Capability token requested from the target agent.</param>
/// <param name="Dependencies">List of node IDs that must finish successfully before this node executes.</param>
/// <param name="EstimatedDurationMilliseconds">Estimated runtime duration for scheduling calculations.</param>
/// <param name="Priority">Scheduling precedence weighting.</param>
/// <param name="RequiredResources">List of strictly mandatory execution system resources.</param>
/// <param name="MaximumResources">Maximum allowable execution resources.</param>
/// <param name="EstimatedMemoryBytes">Estimated volatile RAM requirement in bytes.</param>
/// <param name="EstimatedCPUCores">Estimated processor core ratio requirement.</param>
/// <param name="RetryCount">Number of retry iterations permitted upon transient failure.</param>
/// <param name="RetryDelayMilliseconds">Wait period between retry iterations.</param>
/// <param name="CriticalNode">Whether failure of this node aborts the overall mission plan.</param>
/// <param name="ContinueOnFailure">Whether execution should bypass this node's failure if non-critical.</param>
/// <param name="TimeoutMilliseconds">Max execution timeout in milliseconds before task cancellation.</param>
/// <param name="CheckpointEnabled">Whether state is preserved prior to and after node execution.</param>
/// <param name="RecoveryPolicy">Policy string dictating action upon retry resource exhaustion.</param>
public sealed record ExecutionNode(
    string NodeId,
    string AgentId,
    string CapabilityId,
    IReadOnlyList<string> Dependencies,
    long EstimatedDurationMilliseconds,
    int Priority,
    IReadOnlyList<ExecutionResource> RequiredResources,
    IReadOnlyList<ExecutionResource> MaximumResources,
    long EstimatedMemoryBytes,
    double EstimatedCPUCores,
    int RetryCount,
    long RetryDelayMilliseconds,
    bool CriticalNode,
    bool ContinueOnFailure,
    long TimeoutMilliseconds,
    bool CheckpointEnabled,
    string RecoveryPolicy
)
{
    /// <summary>
    /// Factory helper to build a default configured execution node with reasonable fault-tolerance defaults.
    /// </summary>
    public static ExecutionNode Create(
        string nodeId,
        string agentId,
        string capabilityId,
        IReadOnlyList<string>? dependencies = null,
        long durationMs = 1000L,
        int priority = 1,
        int retryCount = 1,
        bool criticalNode = true,
        bool continueOnFailure = false)
    {
        return new ExecutionNode(
            nodeId,
            agentId,
            capabilityId,
            dependencies ?? Array.Empty<string>(),
            durationMs,
            priority,
            new[] { ExecutionResource.CPU, ExecutionResource.Memory },
            new[] { ExecutionResource.CPU, ExecutionResource.Memory, ExecutionResource.Storage },
            256 * 1024 * 1024L,
            1.0,
            retryCount,
            500L,
            criticalNode,
            continueOnFailure,
            30000L,
            true,
            criticalNode ? "AbortOnExhaustion" : "ContinueOnExhaustion"
        );
    }
}

/// <summary>
/// Encapsulates the verified execution topology and schedule of a multi-agent mission.
/// </summary>
/// <param name="MissionIdentity">Target mission cryptographic identity.</param>
/// <param name="Nodes">Collection of configured execution nodes.</param>
/// <param name="Edges">Directional dependency edges connecting nodes.</param>
/// <param name="ExecutionOrder">Deterministic topological sort order of node identifiers.</param>
/// <param name="TotalEstimatedDurationMilliseconds">Aggregate expected execution runtime across all nodes.</param>
public sealed record ExecutionPlan(
    MissionIdentity MissionIdentity,
    IReadOnlyList<ExecutionNode> Nodes,
    IReadOnlyList<ExecutionEdge> Edges,
    IReadOnlyList<string> ExecutionOrder,
    long TotalEstimatedDurationMilliseconds
);
