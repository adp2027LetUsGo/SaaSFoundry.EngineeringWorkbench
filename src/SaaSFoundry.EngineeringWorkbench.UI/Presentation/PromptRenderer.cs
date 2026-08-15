using System;

namespace SaaSFoundry.EngineeringWorkbench.UI.Presentation;

public class PromptRenderer
{
    private readonly IConsoleRenderer _console;

    public PromptRenderer(IConsoleRenderer console)
    {
        _console = console;
    }

    public void DrawPrompt()
    {
        _console.Write("sfw> ");
    }

    public void DrawSubPrompt(string label)
    {
        _console.Write($"{label}> ");
    }

    public void DrawPostActionMenu()
    {
        _console.WriteLine();
        _console.WriteLine("===========================================================");
        _console.WriteLine("                      Next Actions                        ");
        _console.WriteLine("===========================================================");
        _console.WriteLine();
        _console.WriteLine("1. Return to Dashboard");
        _console.WriteLine("2. Continue Working");
        _console.WriteLine("3. Exit");
        _console.WriteLine();
        _console.WriteLine("===========================================================");
    }
}
