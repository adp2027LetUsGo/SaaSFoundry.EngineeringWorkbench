using System;
using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Application;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Catalog;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;

namespace SaaSFoundry.EngineeringWorkbench.UI.Presentation;

public class SummaryRenderer
{
    private readonly IConsoleRenderer _console;

    public SummaryRenderer(IConsoleRenderer console)
    {
        _console = console;
    }

    public void DrawSummary(
        EngineeringCatalog catalog,
        IReadOnlyList<PlanStorageWrapper> plans,
        IReadOnlyList<ExecutionApproval> approvals,
        IReadOnlyList<ExecutionRecord> reports)
    {
        int pluginCount = catalog.RegisteredPlugins.Count;
        int capCount = catalog.RegisteredPlugins.Sum(p => p.Capabilities.Count);
        int plansGenerated = plans.Count;
        int plansApproved = approvals.Count(a => a.Approved);
        int executionCount = reports.Count;
        int artifactCount = reports.Sum(r => r.Evidence?.ArtifactsGenerated?.Count ?? 0);
        int validationSuccess = reports.Count(r => r.Status == ExecutionStatus.Completed);
        string validationRate = executionCount > 0 ? $"{(validationSuccess * 100 / executionCount)}%" : "N/A";

        _console.DrawSection("Engineering Summary");
        _console.WriteLine($"Engineering Package . pkg-obs-001");
        _console.WriteLine($"Plugins ............. {pluginCount}");
        _console.WriteLine($"Capabilities ........ {capCount}");
        _console.WriteLine($"Plans Generated ..... {plansGenerated}");
        _console.WriteLine($"Plans Approved ...... {plansApproved}");
        _console.WriteLine($"Executions .......... {executionCount}");
        _console.WriteLine($"Artifacts Generated . {artifactCount}");
        _console.WriteLine($"Validation Rate ..... {validationRate}");
        _console.WriteLine($"Catalog Version ..... 1.0.0");
        _console.WriteLine();
    }

    public void DrawLastExecution(IReadOnlyList<ExecutionRecord> reports)
    {
        if (reports.Count == 0) return;

        var last = reports.OrderByDescending(r => r.StartedAt).First();
        var started = DateTimeOffset.FromUnixTimeSeconds(last.StartedAt);
        var completed = last.CompletedAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds(last.CompletedAt.Value) : started;
        var duration = completed - started;
        int artifactCount = last.Evidence?.ArtifactsGenerated?.Count ?? 0;

        _console.DrawSection("Last Execution");
        _console.WriteLine($"Execution ........... {last.ExecutionId}");
        _console.WriteLine($"Status .............. {last.Status}");
        _console.WriteLine($"Plan ................ {last.PlanId}");
        _console.WriteLine($"Duration ............ {duration.TotalSeconds:F1}s");
        _console.WriteLine($"Artifacts Generated . {artifactCount}");
        _console.WriteLine($"Evidence Report ..... output/executions/reports/execution-report-{last.ExecutionId}.json");
        _console.WriteLine();
    }
}
