#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace SaaSFoundry.EngineeringWorkbench.Planning.DAG;

/// <summary>
/// Resolves dependency hierarchy and verifies cyclic consistency across execution DAG nodes.
/// </summary>
public sealed class DependencyResolver
{
    /// <summary>
    /// Validates the DAG topology and returns a deterministic topological sort of Node IDs.
    /// Uses Kahn's Algorithm with lexicographical order tie-breaking for ready nodes.
    /// Throws <see cref="InvalidOperationException"/> if a cycle or missing dependency is detected.
    /// </summary>
    /// <param name="nodes">The candidate execution nodes to resolve.</param>
    /// <returns>An immutable chronological execution ordering of Node IDs.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="nodes"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if a cyclic or broken dependency relationship exists.</exception>
    public IReadOnlyList<string> ResolveDeterministicOrder(IEnumerable<ExecutionNode> nodes)
    {
        if (nodes == null) throw new ArgumentNullException(nameof(nodes));

        var nodeMap = new Dictionary<string, ExecutionNode>(StringComparer.Ordinal);
        foreach (var node in nodes)
        {
            if (!nodeMap.TryAdd(node.NodeId, node))
            {
                throw new InvalidOperationException($"Duplicate node ID discovered during dependency resolution: '{node.NodeId}'.");
            }
        }

        if (nodeMap.Count == 0)
        {
            return Array.Empty<string>();
        }

        var inDegree = new Dictionary<string, int>(StringComparer.Ordinal);
        var adjacency = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var id in nodeMap.Keys)
        {
            inDegree[id] = 0;
            adjacency[id] = new List<string>();
        }

        foreach (var node in nodeMap.Values)
        {
            foreach (var dep in node.Dependencies)
            {
                if (!nodeMap.ContainsKey(dep))
                {
                    throw new InvalidOperationException($"Node '{node.NodeId}' depends on missing node '{dep}'.");
                }
                adjacency[dep].Add(node.NodeId);
                inDegree[node.NodeId]++;
            }
        }

        // Ready queue ordered lexicographically by NodeId for deterministic tie-breaking
        var readySet = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var kvp in inDegree)
        {
            if (kvp.Value == 0)
            {
                readySet.Add(kvp.Key);
            }
        }

        var sortedList = new List<string>(nodeMap.Count);

        while (readySet.Count > 0)
        {
            var current = readySet.Min!;
            readySet.Remove(current);
            sortedList.Add(current);

            foreach (var neighbor in adjacency[current])
            {
                inDegree[neighbor]--;
                if (inDegree[neighbor] == 0)
                {
                    readySet.Add(neighbor);
                }
            }
        }

        if (sortedList.Count != nodeMap.Count)
        {
            var stuck = nodeMap.Keys.Except(sortedList).ToArray();
            throw new InvalidOperationException($"Cyclic dependency detected among nodes: {string.Join(", ", stuck)}");
        }

        return sortedList.ToArray();
    }
}
