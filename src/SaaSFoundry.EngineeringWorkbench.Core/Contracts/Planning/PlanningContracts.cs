using System;
using System.Collections.Generic;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Planning;

public enum ExecutionPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

public sealed record EngineeringRequirement(
    string PluginId,
    string CapabilityId,
    string Operation
);

public sealed record EngineeringTask(
    string TaskId,
    string PluginId,
    string CapabilityId,
    string Operation,
    ExecutionPriority Priority,
    IReadOnlyList<EngineeringRequirement> Dependencies,
    IReadOnlyList<string> ExpectedArtifacts,
    IReadOnlyList<string> ValidationRequirements
);

public sealed record ExecutionStage(
    int StageNumber,
    IReadOnlyList<EngineeringTask> Tasks
);

public sealed record EngineeringPlan(
    string PlanId,
    IReadOnlyList<ExecutionStage> Stages
);

public sealed record PlanningResult(
    bool IsSuccessful,
    EngineeringPlan? Plan,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings,
    DateTimeOffset GeneratedUtc
);

public sealed record EngineeringPlanningContext(
    string Scope,
    IReadOnlyList<EngineeringTask> RequestedTasks
);

public interface IEngineeringPlanner
{
    PlanningResult CreatePlan(EngineeringPlanningContext context);
}
