#nullable enable

using System;
using System.Collections.Generic;

namespace SaaSFoundry.EngineeringWorkbench.Planning.DAG;

/// <summary>
/// Verifies structural and operational consistency of a proposed mission execution DAG topology.
/// </summary>
public sealed class ExecutionTopologyValidator
{
    private readonly DependencyResolver _resolver = new();

    /// <summary>
    /// Evaluates a candidate set of execution nodes against registered agent identities and capability descriptions.
    /// Rejects cyclic graphs, duplicate nodes, duplicate capability assignments, orphan nodes, and missing agent references.
    /// </summary>
    /// <param name="nodes">The candidate nodes to validate.</param>
    /// <param name="registeredAgentIds">Available agent IDs registered in the system.</param>
    /// <param name="availableCapabilityIds">Available capability IDs supported across agents.</param>
    /// <exception cref="ArgumentNullException">Thrown if any argument is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if topology validation rules fail.</exception>
    public void ValidateTopology(
        IEnumerable<ExecutionNode> nodes,
        IEnumerable<string> registeredAgentIds,
        IEnumerable<string> availableCapabilityIds)
    {
        if (nodes == null) throw new ArgumentNullException(nameof(nodes));
        if (registeredAgentIds == null) throw new ArgumentNullException(nameof(registeredAgentIds));
        if (availableCapabilityIds == null) throw new ArgumentNullException(nameof(availableCapabilityIds));

        var nodeList = nodes as IReadOnlyCollection<ExecutionNode> ?? new List<ExecutionNode>(nodes);
        var nodeMap = new Dictionary<string, ExecutionNode>(StringComparer.Ordinal);
        var seenCapabilities = new HashSet<string>(StringComparer.Ordinal);
        var agentSet = new HashSet<string>(registeredAgentIds, StringComparer.Ordinal);
        var capSet = new HashSet<string>(availableCapabilityIds, StringComparer.Ordinal);

        foreach (var node in nodeList)
        {
            // Reject duplicate NodeIds or duplicate capabilities across nodes in the mission plan
            if (nodeMap.ContainsKey(node.NodeId))
            {
                throw new InvalidOperationException($"Duplicate node identifier detected: '{node.NodeId}'.");
            }
            if (!seenCapabilities.Add(node.CapabilityId))
            {
                throw new InvalidOperationException($"Duplicate capability assignment in mission DAG: '{node.CapabilityId}'.");
            }

            // Reject missing agents
            if (!agentSet.Contains(node.AgentId))
            {
                throw new InvalidOperationException($"Node '{node.NodeId}' targets unregistered agent '{node.AgentId}'.");
            }

            // Reject missing capabilities
            if (capSet.Count > 0 && !capSet.Contains(node.CapabilityId))
            {
                throw new InvalidOperationException($"Node '{node.NodeId}' targets unsupported capability '{node.CapabilityId}'.");
            }

            nodeMap[node.NodeId] = node;
        }

        // Validate missing dependencies and reject cycles via deterministic sort
        _ = _resolver.ResolveDeterministicOrder(nodeList);

        // Reject orphan / unreachable nodes
        // An orphan node in a multi-node DAG is one that has zero incoming dependencies and zero outgoing dependents,
        // and is disconnected from the main mission goal when multiple disconnected components exist.
        if (nodeList.Count > 1)
        {
            var hasIncoming = new HashSet<string>(StringComparer.Ordinal);
            var hasOutgoing = new HashSet<string>(StringComparer.Ordinal);

            foreach (var node in nodeList)
            {
                foreach (var dep in node.Dependencies)
                {
                    hasIncoming.Add(node.NodeId);
                    hasOutgoing.Add(dep);
                }
            }

            foreach (var node in nodeList)
            {
                if (!hasIncoming.Contains(node.NodeId) && !hasOutgoing.Contains(node.NodeId))
                {
                    throw new InvalidOperationException($"Orphan node detected: '{node.NodeId}' is disconnected from all dependency paths in multi-node mission.");
                }
            }
        }
    }
}
