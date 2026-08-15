#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Execution;
using SaaSFoundry.EngineeringWorkbench.Execution.Capabilities;

namespace SaaSFoundry.EngineeringWorkbench.Planning.DAG;

/// <summary>
/// Responsible for validating candidate nodes and building deterministic execution plans over immutable DAGs.
/// </summary>
public sealed class MissionPlanner
{
    private readonly ExecutionTopologyValidator _validator = new();
    private readonly DependencyResolver _resolver = new();

    /// <summary>
    /// Plans a mission execution DAG deterministically from candidate execution nodes and available capabilities.
    /// Rejects cycles, missing dependencies, duplicate nodes, or unregistered agents.
    /// </summary>
    /// <param name="context">The initialized runtime mission execution context.</param>
    /// <param name="configuredNodes">The set of nodes requested for mission execution.</param>
    /// <param name="availableCapabilities">Optional collection of verified capability descriptors.</param>
    /// <returns>A verified, immutable <see cref="ExecutionPlan"/> containing sorted topological execution order.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> or <paramref name="configuredNodes"/> is null.</exception>
    public ExecutionPlan CreatePlan(
        MissionExecutionContext context,
        IEnumerable<ExecutionNode> configuredNodes,
        IEnumerable<AgentCapabilityDescriptor>? availableCapabilities = null)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (configuredNodes == null) throw new ArgumentNullException(nameof(configuredNodes));

        var nodeList = configuredNodes as IReadOnlyList<ExecutionNode> ?? configuredNodes.ToArray();
        var agentIds = new List<string>();

        foreach (var node in nodeList)
        {
            if (context.AgentRegistry.TryGetAgent(node.AgentId, out _))
            {
                agentIds.Add(node.AgentId);
            }
        }

        var capIds = availableCapabilities?.Select(c => c.CapabilityId) ?? Array.Empty<string>();

        _validator.ValidateTopology(nodeList, agentIds, capIds);

        var ordering = _resolver.ResolveDeterministicOrder(nodeList);
        var edges = new List<ExecutionEdge>();
        long totalDuration = 0;

        foreach (var node in nodeList)
        {
            totalDuration += node.EstimatedDurationMilliseconds;
            foreach (var dep in node.Dependencies)
            {
                edges.Add(new ExecutionEdge(dep, node.NodeId));
            }
        }

        // Sort edges deterministically
        edges.Sort((a, b) =>
        {
            int cmp = string.Compare(a.SourceNodeId, b.SourceNodeId, StringComparison.Ordinal);
            return cmp != 0 ? cmp : string.Compare(a.TargetNodeId, b.TargetNodeId, StringComparison.Ordinal);
        });

        context.MissionBlackboard.AddHistoryEntry($"[MissionPlanner] Deterministic DAG plan created with {nodeList.Count} nodes and {edges.Count} dependency edges.");
        context.ExecutionClock.AdvanceMilliseconds(50);

        return new ExecutionPlan(context.MissionIdentity, nodeList, edges.ToArray(), ordering, totalDuration);
    }
}
