using System;

namespace SaaSFoundry.EngineeringWorkbench.UI.Presentation;

public class HelpRenderer
{
    private readonly IConsoleRenderer _console;

    public HelpRenderer(IConsoleRenderer console)
    {
        _console = console;
    }

    public void DrawHelp()
    {
        _console.DrawHeader("Engineering Workbench Help");

        _console.DrawSection("Interactive Mode");
        _console.WriteLine("Run 'sfw' with no arguments to start the interactive workspace.");
        _console.WriteLine("Use numbered menu options or type commands directly at the sfw> prompt.");
        _console.WriteLine();

        _console.DrawSection("Menu Navigation");
        _console.WriteLine("1. Create Engineering Plan    6. Browse Plugins");
        _console.WriteLine("2. Review Plans               7. Browse Capabilities");
        _console.WriteLine("3. Approve Plan               8. Doctor");
        _console.WriteLine("4. Execute Plan               9. Help");
        _console.WriteLine("5. Review Reports             0. Exit");
        _console.WriteLine();

        _console.DrawSection("Command Mode");
        _console.WriteLine("Commands can be used both in interactive mode and from the terminal:");
        _console.WriteLine();
        _console.WriteLine("    sfw plan observability");
        _console.WriteLine("    sfw plans");
        _console.WriteLine("    sfw execute approve <planId>");
        _console.WriteLine("    sfw execute plan <planId>");
        _console.WriteLine("    sfw execute report <executionId>");
        _console.WriteLine("    sfw plugins");
        _console.WriteLine("    sfw capabilities");
        _console.WriteLine("    sfw info <plugin>");
        _console.WriteLine("    sfw summary");
        _console.WriteLine("    sfw doctor");
        _console.WriteLine("    sfw catalog validate");
        _console.WriteLine();

        _console.DrawSection("Engineering Workflow");
        _console.WriteLine("1. Plan    → Generate an Engineering Plan from a package");
        _console.WriteLine("2. Approve → Authorize execution via governance");
        _console.WriteLine("3. Execute → Run the approved plan through the engine");
        _console.WriteLine("4. Review  → Inspect execution evidence and reports");
        _console.WriteLine();

        _console.DrawSection("Engineering Concepts");
        _console.WriteLine("- Engineering Package: Definition of plugins, capabilities, and dependencies.");
        _console.WriteLine("- Engineering Plan: A deterministic DAG of tasks to be executed.");
        _console.WriteLine("- Execution: An approved enactment of a Plan by the Workbench Engine.");
        _console.WriteLine("- Evidence: Output artifacts, validation results, and logs.");
        _console.WriteLine();

        _console.DrawSection("Examples");
        _console.WriteLine("  Terminal:      sfw plan observability");
        _console.WriteLine("  Interactive:   Type '1' at the sfw> prompt for guided plan creation");
        _console.WriteLine("  Quick access:  Type 'plugins' or '6' at the sfw> prompt");
        _console.WriteLine();

        _console.DrawSection("Troubleshooting");
        _console.WriteLine("Run 'sfw doctor' or type '8' in interactive mode.");
        _console.WriteLine();
    }
}
