using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Catalog;
using SaaSFoundry.EngineeringWorkbench.Application;

namespace SaaSFoundry.EngineeringWorkbench.UI.Presentation;

public class WorkflowRenderer
{
    private readonly IConsoleRenderer _console;
    private readonly PromptRenderer _prompt;

    public WorkflowRenderer(IConsoleRenderer console, PromptRenderer prompt)
    {
        _console = console;
        _prompt = prompt;
    }

    public string? GuidePlanCreation(EngineeringCatalog catalog)
    {
        _console.DrawSection("Create Engineering Plan");
        _console.WriteLine();
        _console.WriteLine("Available Engineering Packages:");

        var plugins = catalog.RegisteredPlugins.ToList();
        for (int i = 0; i < plugins.Count; i++)
        {
            _console.WriteLine($"  {i + 1}. {plugins[i].PluginId}");
        }
        _console.WriteLine();
        _prompt.DrawSubPrompt("Select package");

        var input = Console.ReadLine();
        if (input == null) return null;
        input = input.Trim();
        if (string.IsNullOrEmpty(input)) return null;

        if (int.TryParse(input, out int idx) && idx >= 1 && idx <= plugins.Count)
        {
            return plugins[idx - 1].PluginId;
        }

        return input;
    }

    public string? GuideApproval(IReadOnlyList<PlanStorageWrapper> plans, IReadOnlyList<Core.Contracts.Governance.ExecutionApproval> approvals)
    {
        _console.DrawSection("Approve Plan");
        _console.WriteLine();
        _console.WriteLine("Available Plans:");

        var planList = plans.Where(p => p.Plan != null).ToList();
        if (planList.Count == 0)
        {
            _console.DrawWarning("No plans available. Create one first with 'plan'.");
            return null;
        }

        for (int i = 0; i < planList.Count; i++)
        {
            var p = planList[i];
            var approval = approvals.FirstOrDefault(a => a.PlanId == p.Plan!.PlanId);
            bool isApproved = approval != null && approval.Approved;
            _console.WriteLine($"  {i + 1}. {p.Plan!.PlanId}  [{(isApproved ? "Approved" : "Pending")}]");
        }
        _console.WriteLine();
        _prompt.DrawSubPrompt("Select Plan");

        var input = Console.ReadLine();
        if (input == null) return null;
        input = input.Trim();
        if (string.IsNullOrEmpty(input)) return null;
        string? planId = null;
        if (int.TryParse(input, out int idx) && idx >= 1 && idx <= planList.Count)
        {
            planId = planList[idx - 1].Plan!.PlanId;
        }
        else
        {
            planId = input;
        }

        _console.WriteLine();
        _console.Write($"Approve plan '{planId}'? (Y/N) ");
        var confirm = Console.ReadLine();
        if (confirm == null) return null;
        if (confirm.Trim().ToUpperInvariant() != "Y") return null;

        return planId;
    }

    public string? GuideExecution(IReadOnlyList<PlanStorageWrapper> plans, IReadOnlyList<Core.Contracts.Governance.ExecutionApproval> approvals)
    {
        _console.DrawSection("Execute Plan");
        _console.WriteLine();
        _console.WriteLine("Approved Plans:");

        var approved = plans
            .Where(p => p.Plan != null && approvals.Any(a => a.PlanId == p.Plan.PlanId && a.Approved))
            .ToList();

        if (approved.Count == 0)
        {
            _console.DrawWarning("No approved plans available. Approve one first.");
            return null;
        }

        for (int i = 0; i < approved.Count; i++)
        {
            _console.WriteLine($"  {i + 1}. {approved[i].Plan!.PlanId}");
        }
        _console.WriteLine();
        _prompt.DrawSubPrompt("Select Plan");

        var input = Console.ReadLine();
        if (input == null) return null;
        input = input.Trim();
        if (string.IsNullOrEmpty(input)) return null;

        if (int.TryParse(input, out int idx) && idx >= 1 && idx <= approved.Count)
        {
            return approved[idx - 1].Plan!.PlanId;
        }

        return input;
    }

    public string? GuideReportSelection(IReadOnlyList<Core.Contracts.Governance.ExecutionRecord> reports)
    {
        _console.DrawSection("Review Reports");
        _console.WriteLine();

        if (reports.Count == 0)
        {
            _console.DrawWarning("No execution reports available.");
            return null;
        }

        _console.WriteLine("Available Reports:");
        var sorted = reports.OrderByDescending(r => r.StartedAt).ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            _console.WriteLine($"  {i + 1}. {sorted[i].ExecutionId} [{sorted[i].Status}]");
        }
        _console.WriteLine();
        _prompt.DrawSubPrompt("Select Report");

        var input = Console.ReadLine();
        if (input == null) return null;
        input = input.Trim();
        if (string.IsNullOrEmpty(input)) return null;

        if (int.TryParse(input, out int idx) && idx >= 1 && idx <= sorted.Count)
        {
            return sorted[idx - 1].ExecutionId;
        }

        return input;
    }
}
