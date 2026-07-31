using SaaSFoundry.EngineeringWorkbench.Cli.Commands;

namespace SaaSFoundry.EngineeringWorkbench.Cli.Services;

public sealed class CommandRouter
{
    private readonly PluginCommands _pluginCommands = new();

    public async Task ExecuteAsync(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return;
        }

        var command = args[0].ToLowerInvariant();
        switch(command)
        {
            case "list":
                await _pluginCommands.ListAsync();
                break;

            case "generate":
                var plugin = args.Length > 1 ? args[1] : "observability";
                await _pluginCommands.GenerateAsync(plugin);
                break;

            case "validate":
                await _pluginCommands.ValidateAsync();
                break;

            case "report":
                await _pluginCommands.ReportAsync();
                break;

            default:
                ShowHelp();
                break;
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("\""
SaaSFoundry EngineeringWorkbench CLI

Commands:

 list
 generate <plugin>
 validate
 report
"\"");
    }
}
