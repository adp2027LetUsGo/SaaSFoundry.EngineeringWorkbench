#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Execution;
using SaaSFoundry.EngineeringWorkbench.Execution.Strategies;
using SaaSFoundry.EngineeringWorkbench.Planning.DAG;

namespace SaaSFoundry.EngineeringWorkbench.Planning.Scheduling;

/// <summary>
/// Schedules and coordinates execution of DAG mission plans according to configured concurrency strategies.
/// </summary>
public sealed class ExecutionScheduler
{
    private readonly IExecutionStrategy _strategy;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionScheduler"/> class with the specified execution strategy.
    /// </summary>
    /// <param name="strategy">The concurrency strategy (defaults to sequential if null).</param>
    public ExecutionScheduler(IExecutionStrategy strategy)
    {
        _strategy = strategy ?? new SequentialExecutionStrategy();
    }

    /// <summary>
    /// Schedules and executes nodes over the DAG according to the execution strategy and resource concurrency limits.
    /// Never violates dependency ordering. Supports retry loops and critical node failure recovery.
    /// </summary>
    /// <param name="plan">The verified DAG execution plan.</param>
    /// <param name="context">The active mission runtime execution context.</param>
    /// <param name="executeNodeAsync">The worker dispatch callback invoked for each node.</param>
    /// <param name="cancellationToken">Optional token to signal premature termination.</param>
    /// <returns>True if all nodes completed successfully without aborting critical branches; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if required parameters are null.</exception>
    public async Task<bool> ScheduleAndExecuteAsync(
        ExecutionPlan plan,
        MissionExecutionContext context,
        Func<ExecutionNode, CancellationToken, Task<bool>> executeNodeAsync,
        CancellationToken cancellationToken = default)
    {
        if (plan == null) throw new ArgumentNullException(nameof(plan));
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (executeNodeAsync == null) throw new ArgumentNullException(nameof(executeNodeAsync));

        var nodeMap = new Dictionary<string, ExecutionNode>(StringComparer.Ordinal);
        foreach (var n in plan.Nodes)
        {
            nodeMap[n.NodeId] = n;
        }

        var completedNodes = new HashSet<string>(StringComparer.Ordinal);
        var runningNodes = new Dictionary<string, Task<bool>>(StringComparer.Ordinal);
        var inDegree = new Dictionary<string, int>(StringComparer.Ordinal);
        var adjacency = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var id in plan.ExecutionOrder)
        {
            inDegree[id] = 0;
            adjacency[id] = new List<string>();
        }

        foreach (var edge in plan.Edges)
        {
            adjacency[edge.SourceNodeId].Add(edge.TargetNodeId);
            inDegree[edge.TargetNodeId]++;
        }

        var readyQueue = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var kvp in inDegree)
        {
            if (kvp.Value == 0)
            {
                readyQueue.Add(kvp.Key);
            }
        }

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, context.CancellationToken);
        int maxConcurrency = _strategy.AllowParallel ? _strategy.MaxConcurrency : 1;
        bool missionAborted = false;

        while ((readyQueue.Count > 0 || runningNodes.Count > 0) && !missionAborted && !linkedCts.IsCancellationRequested)
        {
            // Dispatch nodes from ready queue up to max concurrency limit
            while (readyQueue.Count > 0 && runningNodes.Count < maxConcurrency && !missionAborted)
            {
                var nextId = readyQueue.Min!;
                readyQueue.Remove(nextId);
                var node = nodeMap[nextId];

                // Execute with retry loops according to node failure policy
                runningNodes[nextId] = ExecuteWithRetryAsync(node, context, executeNodeAsync, linkedCts.Token);
            }

            if (runningNodes.Count > 0)
            {
                // Wait for the first node to finish execution
                var firstFinishedTask = await Task.WhenAny(runningNodes.Values);
                string? finishedKey = null;
                foreach (var kvp in runningNodes)
                {
                    if (kvp.Value == firstFinishedTask)
                    {
                        finishedKey = kvp.Key;
                        break;
                    }
                }

                if (finishedKey != null)
                {
                    runningNodes.Remove(finishedKey);
                    bool success = await firstFinishedTask;
                    var finishedNode = nodeMap[finishedKey];

                    if (!success)
                    {
                        context.MissionBlackboard.AddDiagnostic($"[ExecutionScheduler] Node '{finishedNode.NodeId}' failed after exhausting retries.");
                        if (finishedNode.CriticalNode && !finishedNode.ContinueOnFailure)
                        {
                            context.MissionBlackboard.AddDiagnostic($"[ExecutionScheduler] Critical node '{finishedNode.NodeId}' failure aborts remaining DAG execution.");
                            missionAborted = true;
                            linkedCts.Cancel();
                            break;
                        }
                    }
                    else
                    {
                        completedNodes.Add(finishedNode.NodeId);
                        foreach (var neighbor in adjacency[finishedNode.NodeId])
                        {
                            inDegree[neighbor]--;
                            if (inDegree[neighbor] == 0 && !missionAborted)
                            {
                                readyQueue.Add(neighbor);
                            }
                        }
                    }
                }
            }
        }

        return !missionAborted && !linkedCts.IsCancellationRequested && completedNodes.Count == plan.Nodes.Count;
    }

    private async Task<bool> ExecuteWithRetryAsync(
        ExecutionNode node,
        MissionExecutionContext context,
        Func<ExecutionNode, CancellationToken, Task<bool>> executeFunc,
        CancellationToken token)
    {
        int attempts = 0;
        int maxAttempts = Math.Max(1, node.RetryCount + 1);

        while (attempts < maxAttempts && !token.IsCancellationRequested)
        {
            attempts++;
            try
            {
                bool result = await executeFunc(node, token);
                if (result)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                context.MissionBlackboard.AddDiagnostic($"[ExecutionScheduler] Exception executing '{node.NodeId}' on attempt {attempts}: {ex.Message}");
            }

            if (attempts < maxAttempts && !token.IsCancellationRequested)
            {
                context.MissionBlackboard.AddHistoryEntry($"[ExecutionScheduler] Retrying node '{node.NodeId}' in {node.RetryDelayMilliseconds}ms (Attempt {attempts} of {maxAttempts}).");
                if (node.RetryDelayMilliseconds > 0 && !context.ExecutionClock.IsDeterministic)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(node.RetryDelayMilliseconds), token);
                }
                else
                {
                    context.ExecutionClock.AdvanceMilliseconds(node.RetryDelayMilliseconds);
                }
            }
        }

        return false;
    }
}
