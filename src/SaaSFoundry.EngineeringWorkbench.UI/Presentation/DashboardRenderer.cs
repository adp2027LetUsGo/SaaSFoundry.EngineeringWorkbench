using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Application;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Catalog;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;

namespace SaaSFoundry.EngineeringWorkbench.UI.Presentation;

public class DashboardRenderer
{
    private readonly IConsoleRenderer _console;
    private readonly ActivityRenderer _activity;
    private readonly SummaryRenderer _summary;
    private readonly NextActionRenderer _nextAction;

    public DashboardRenderer(IConsoleRenderer console)
    {
        _console = console;
        _activity = new ActivityRenderer(console);
        _summary = new SummaryRenderer(console);
        _nextAction = new NextActionRenderer(console);
    }

    public void DrawDashboard(
        EngineeringCatalog catalog,
        IReadOnlyList<PlanStorageWrapper> plans,
        IReadOnlyList<ExecutionApproval> approvals,
        IReadOnlyList<ExecutionRecord> reports)
    {
        int pluginCount = catalog.RegisteredPlugins.Count;
        int capCount = catalog.RegisteredPlugins.Sum(p => p.Capabilities.Count);
        bool hasAssets = plans.Count > 0 || reports.Count > 0;

        _console.DrawHeader("SaaSFoundry Engineering Workbench v1.0");

        // Workspace Section
        var lastPlan = plans.OrderByDescending(p => p.Identity?.CreatedTimestamp ?? 0).FirstOrDefault();
        var lastReport = reports.OrderByDescending(r => r.StartedAt).FirstOrDefault();

        _console.DrawSection("Current Workspace");
        _console.WriteLine($"Engineering Package . pkg-obs-001");
        _console.WriteLine($"Catalog Version ..... 1.0.0");
        _console.WriteLine($"Canonical Version ... 1.0.0");
        _console.WriteLine($"Working Directory ... {Directory.GetCurrentDirectory()}");

        bool outDirOk = Directory.Exists("output/executions");
        _console.WriteLine($"Output Folder ....... {(outDirOk ? "output/executions" : "MISSING")}");

        _console.WriteLine($"Last Plan ........... {(lastPlan?.Plan != null ? lastPlan.Plan.PlanId : "None")}");
        _console.WriteLine($"Last Execution ...... {(lastReport != null ? lastReport.ExecutionId : "None")}");
        _console.WriteLine();

        // Platform Section
        _console.DrawSection("Platform");
        _console.WriteLine($"Catalog ............. {CliRenderer.IconSuccess} Ready");
        _console.WriteLine($"Planner ............. {CliRenderer.IconSuccess} Ready");
        _console.WriteLine($"Governance .......... {CliRenderer.IconSuccess} Ready");
        _console.WriteLine();

        if (!hasAssets)
        {
            // Empty Workspace Experience
            DrawEmptyWorkspace();
            return;
        }

        // Engineering Assets Section
        int pendingPlans = plans.Count(p => p.Plan != null && !approvals.Any(a => a.PlanId == p.Plan.PlanId && a.Approved));
        _console.DrawSection("Engineering Assets");
        _console.WriteLine($"Plugins ............. {pluginCount}");
        _console.WriteLine($"Capabilities ........ {capCount}");
        _console.WriteLine($"Plans ............... {plans.Count}");
        _console.WriteLine($"Pending Approval .... {pendingPlans}");
        _console.WriteLine($"Executions .......... {reports.Count}");
        _console.WriteLine();

        // Recent Activity
        _activity.DrawRecentActivity(plans, approvals, reports);

        // Last Execution Summary
        _summary.DrawLastExecution(reports);

        // Context-aware Next Actions
        _nextAction.DrawNextActions(plans, approvals, reports);
    }

    private void DrawEmptyWorkspace()
    {
        _console.DrawSection("No Engineering Workspace detected");
        _console.WriteLine();
        _console.WriteLine("Suggested first steps:");
        _console.WriteLine();
        _console.WriteLine("1.");
        _console.WriteLine("    sfw catalog validate");
        _console.WriteLine("2.");
        _console.WriteLine("    sfw plugins");
        _console.WriteLine("3.");
        _console.WriteLine("    sfw plan observability");
        _console.WriteLine("4.");
        _console.WriteLine("    sfw help");
    }
}
