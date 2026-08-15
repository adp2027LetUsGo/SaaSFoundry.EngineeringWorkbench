using System;
using System.Collections.Generic;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Planning;

namespace SaaSFoundry.Plugins.Observability.Planner;

public static class ObservabilityPlanContributor
{
    public static string ContributorId => "observability-planner-contributor";

    public static IReadOnlyList<EngineeringTask> GetRequestedTasks(string scope = "observability")
    {
        var capabilityDefinitions = new[]
        {
            ("logging", (string?)null),
            ("metrics", "logging"),
            ("tracing", "metrics"),
            ("healthchecks", "tracing"),
            ("collector", "healthchecks"),
            ("configuration", "collector"),
            ("dashboards", "configuration"),
            ("alerts", "dashboards"),
            ("documentation", "alerts"),
            ("validation", "documentation")
        };

        var tasks = new List<EngineeringTask>(capabilityDefinitions.Length);
        int taskNumber = 1;

        foreach (var (id, reqId) in capabilityDefinitions)
        {
            var dependencies = reqId != null
                ? new List<EngineeringRequirement> { new EngineeringRequirement("observability", reqId, "generate") }
                : new List<EngineeringRequirement>();

            var expectedArtifacts = new List<string>
            {
                $"OBS-{taskNumber + 2:D3}-{id.Substring(0, 1).ToUpper() + id.Substring(1)}-Standards.md",
                $"{id}-config.json"
            };

            var validationReqs = new List<string> { $"{id}-configuration-validation", $"{id}-execution-validation" };

            tasks.Add(new EngineeringTask(
                TaskId: $"task-{taskNumber++}-{id}",
                PluginId: "observability",
                CapabilityId: id,
                Operation: "generate",
                Priority: ExecutionPriority.Normal,
                Dependencies: dependencies,
                ExpectedArtifacts: expectedArtifacts,
                ValidationRequirements: validationReqs
            ));
        }

        return tasks;
    }
}
