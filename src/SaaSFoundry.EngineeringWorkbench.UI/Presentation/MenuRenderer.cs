using System;

namespace SaaSFoundry.EngineeringWorkbench.UI.Presentation;

public class MenuRenderer
{
    private readonly IConsoleRenderer _console;

    public MenuRenderer(IConsoleRenderer console)
    {
        _console = console;
    }

    public void DrawMenu()
    {
        _console.WriteLine("===========================================================");
        _console.WriteLine("                   Engineering Workflow                    ");
        _console.WriteLine("===========================================================");
        _console.WriteLine();
        _console.WriteLine("1. Create Engineering Plan");
        _console.WriteLine("2. Review Plans");
        _console.WriteLine("3. Approve Plan");
        _console.WriteLine("4. Execute Plan");
        _console.WriteLine("5. Review Reports");
        _console.WriteLine("6. Browse Plugins");
        _console.WriteLine("7. Browse Capabilities");
        _console.WriteLine("8. Doctor");
        _console.WriteLine("9. Help");
        _console.WriteLine("0. Exit");
        _console.WriteLine();
        _console.WriteLine("===========================================================");
    }
}
