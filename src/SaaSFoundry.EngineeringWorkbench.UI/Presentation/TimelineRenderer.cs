using System;
using System.Collections.Generic;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;

namespace SaaSFoundry.EngineeringWorkbench.UI.Presentation;

public class TimelineRenderer
{
    private readonly IConsoleRenderer _console;

    public TimelineRenderer(IConsoleRenderer console)
    {
        _console = console;
    }

    public void DrawTimeline(ExecutionRecord record)
    {
        _console.DrawSection("Execution Timeline");
        
        var started = DateTimeOffset.FromUnixTimeSeconds(record.StartedAt);
        var completed = record.CompletedAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds(record.CompletedAt.Value) : started;

        _console.WriteLine($"{started:HH:mm:ss}  Plan Loaded");
        _console.WriteLine($"{started:HH:mm:ss}  Governance Approved");
        _console.WriteLine($"{started:HH:mm:ss}  Stage 1 Started");

        foreach (var task in record.ExecutedTasks)
        {
            _console.WriteLine($"{started:HH:mm:ss}  {task}");
        }

        _console.WriteLine($"{completed:HH:mm:ss}  Validation");
        _console.WriteLine($"{completed:HH:mm:ss}  Packaging");
        _console.WriteLine($"{completed:HH:mm:ss}  Completed");
        _console.WriteLine();
    }
}
