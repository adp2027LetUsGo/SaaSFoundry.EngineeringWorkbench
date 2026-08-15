using System;
using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Application;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;

namespace SaaSFoundry.EngineeringWorkbench.UI.Presentation;

public class NextActionRenderer
{
    private readonly IConsoleRenderer _console;

    public NextActionRenderer(IConsoleRenderer console)
    {
        _console = console;
    }

    public void DrawNextActions(
        IReadOnlyList<PlanStorageWrapper> plans,
        IReadOnlyList<ExecutionApproval> approvals,
        IReadOnlyList<ExecutionRecord> reports)
    {
        _console.DrawSection("Next Actions");

        if (plans.Count == 0)
        {
            _console.WriteLine($"{CliRenderer.IconInfo} No plans exist. Create your first plan:");
            _console.WriteLine("    sfw plan observability");
            return;
        }

        var unapproved = plans
            .Where(p => p.Plan != null && !approvals.Any(a => a.PlanId == p.Plan.PlanId && a.Approved))
            .ToList();

        if (unapproved.Any())
        {
            var planId = unapproved.First().Plan!.PlanId;
            _console.WriteLine($"{CliRenderer.IconInfo} You have {unapproved.Count} plan(s) awaiting approval:");
            _console.WriteLine($"    sfw execute approve {planId}");
            return;
        }

        var approvedNotExecuted = plans
            .Where(p => p.Plan != null
                && approvals.Any(a => a.PlanId == p.Plan.PlanId && a.Approved)
                && !reports.Any(r => r.PlanId == p.Plan.PlanId && r.Status == ExecutionStatus.Completed))
            .ToList();

        if (approvedNotExecuted.Any())
        {
            var planId = approvedNotExecuted.First().Plan!.PlanId;
            _console.WriteLine($"{CliRenderer.IconInfo} Approved plan ready for execution:");
            _console.WriteLine($"    sfw execute plan {planId}");
            return;
        }

        if (reports.Any())
        {
            var last = reports.OrderByDescending(r => r.StartedAt).First();
            _console.WriteLine($"{CliRenderer.IconSuccess} All plans executed. Review latest evidence:");
            _console.WriteLine($"    sfw execute report {last.ExecutionId}");
            _console.WriteLine("Or create a new plan:");
            _console.WriteLine("    sfw plan observability");
            return;
        }

        _console.WriteLine("    sfw plan observability");
    }
}
