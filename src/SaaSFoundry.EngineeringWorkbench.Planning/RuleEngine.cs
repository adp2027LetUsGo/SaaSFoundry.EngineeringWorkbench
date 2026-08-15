using System;
using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Planning;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Catalog;

namespace SaaSFoundry.EngineeringWorkbench.Planning;

internal static class RuleEngine
{
    public static PlanningResult CalculateExecutionPlan(EngineeringPlanningContext context, EngineeringCatalog catalog)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var planId = "plan-" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (!catalog.ValidationResult.IsSuccessful)
        {
            errors.Add("Cannot plan execution. Catalog is invalid.");
            return new PlanningResult(false, null, errors, warnings, DateTimeOffset.UtcNow);
        }

        if (context.RequestedTasks == null || context.RequestedTasks.Count == 0)
        {
            errors.Add("No tasks provided in the planning context.");
            return new PlanningResult(false, null, errors, warnings, DateTimeOffset.UtcNow);
        }

        // Validate that requested tasks exist in the catalog
        foreach (var task in context.RequestedTasks)
        {
            var plugin = catalog.RegisteredPlugins.FirstOrDefault(p => p.PluginId.Equals(task.PluginId, StringComparison.OrdinalIgnoreCase));
            if (plugin == null)
            {
                errors.Add($"Requested plugin '{task.PluginId}' does not exist in catalog.");
                continue;
            }

            var cap = plugin.Capabilities.FirstOrDefault(c => c.CapabilityId.Equals(task.CapabilityId, StringComparison.OrdinalIgnoreCase) && c.Operation.Equals(task.Operation, StringComparison.OrdinalIgnoreCase));
            if (cap == null)
            {
                errors.Add($"Requested capability '{task.CapabilityId}' (Op: {task.Operation}) does not exist in plugin '{task.PluginId}'.");
            }
        }

        if (errors.Any())
        {
            return new PlanningResult(false, null, errors, warnings, DateTimeOffset.UtcNow);
        }

        // 1. Build dependency graph
        var graph = new Dictionary<string, List<string>>();
        var inDegree = new Dictionary<string, int>();
        var taskLookup = new Dictionary<string, EngineeringTask>();

        foreach (var task in context.RequestedTasks)
        {
            if (!taskLookup.ContainsKey(task.TaskId))
            {
                taskLookup[task.TaskId] = task;
                graph[task.TaskId] = new List<string>();
                inDegree[task.TaskId] = 0;
            }
        }

        foreach (var task in context.RequestedTasks)
        {
            foreach (var dep in task.Dependencies)
            {
                var depTask = context.RequestedTasks.FirstOrDefault(t => 
                    t.PluginId.Equals(dep.PluginId, StringComparison.OrdinalIgnoreCase) && 
                    t.CapabilityId.Equals(dep.CapabilityId, StringComparison.OrdinalIgnoreCase) &&
                    t.Operation.Equals(dep.Operation, StringComparison.OrdinalIgnoreCase));
                
                if (depTask == null)
                {
                    errors.Add($"Missing dependency in context: Task '{task.TaskId}' depends on '{dep.PluginId}.{dep.CapabilityId}.{dep.Operation}' which is not in the context.");
                    continue;
                }

                graph[depTask.TaskId].Add(task.TaskId);
                inDegree[task.TaskId]++;
            }
        }

        if (errors.Any())
        {
            return new PlanningResult(false, null, errors, warnings, DateTimeOffset.UtcNow);
        }

        // 2. Kahn's Algorithm for Topological Sort & Stage Generation
        var queue = new Queue<string>();
        foreach (var node in inDegree)
        {
            if (node.Value == 0)
            {
                queue.Enqueue(node.Key);
            }
        }

        var stages = new List<ExecutionStage>();
        int stageCounter = 1;
        int processedCount = 0;

        while (queue.Count > 0)
        {
            var stageSize = queue.Count;
            var currentStageTasks = new List<EngineeringTask>();

            for (int i = 0; i < stageSize; i++)
            {
                var current = queue.Dequeue();
                currentStageTasks.Add(taskLookup[current]);
                processedCount++;

                foreach (var neighbor in graph[current])
                {
                    inDegree[neighbor]--;
                    if (inDegree[neighbor] == 0)
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            stages.Add(new ExecutionStage(stageCounter++, currentStageTasks));
        }

        if (processedCount != context.RequestedTasks.Count)
        {
            errors.Add("Cycle detected in task dependencies. Cannot produce a valid execution plan.");
            return new PlanningResult(false, null, errors, warnings, DateTimeOffset.UtcNow);
        }

        var plan = new EngineeringPlan(planId, stages);
        return new PlanningResult(true, plan, errors, warnings, DateTimeOffset.UtcNow);
    }
}
