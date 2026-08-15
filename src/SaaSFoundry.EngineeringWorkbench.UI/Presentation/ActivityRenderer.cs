using System;
using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Application;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;

namespace SaaSFoundry.EngineeringWorkbench.UI.Presentation;

public class ActivityRenderer
{
    private readonly IConsoleRenderer _console;

    public ActivityRenderer(IConsoleRenderer console)
    {
        _console = console;
    }

    public void DrawRecentActivity(
        IReadOnlyList<PlanStorageWrapper> plans,
        IReadOnlyList<ExecutionApproval> approvals,
        IReadOnlyList<ExecutionRecord> reports)
    {
        var events = new List<(long Timestamp, string Description)>();

        foreach (var p in plans)
        {
            if (p.Plan == null || p.Identity == null) continue;
            events.Add((p.Identity.CreatedTimestamp, $"Generated Plan\n        {p.Plan.PlanId}"));
        }

        foreach (var a in approvals)
        {
            events.Add((a.ApprovedTimestamp, $"Approved Plan\n        {a.PlanId}"));
        }

        foreach (var r in reports)
        {
            events.Add((r.StartedAt, $"Executed Plan\n        {r.ExecutionId}"));
            if (r.Evidence != null && r.Evidence.ArtifactsGenerated.Count > 0)
            {
                var completedAt = r.CompletedAt ?? r.StartedAt;
                events.Add((completedAt, "Generated Evidence"));
            }
        }

        var sorted = events.OrderByDescending(e => e.Timestamp).Take(10).ToList();

        if (sorted.Count == 0) return;

        _console.DrawSection("Recent Activity");
        foreach (var ev in sorted)
        {
            var time = DateTimeOffset.FromUnixTimeSeconds(ev.Timestamp).ToString("HH:mm");
            _console.WriteLine($"{time}  {ev.Description}");
            _console.WriteLine();
        }
    }
}
