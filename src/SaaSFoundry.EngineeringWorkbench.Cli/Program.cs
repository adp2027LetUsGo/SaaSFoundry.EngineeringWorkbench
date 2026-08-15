using System;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Cli.Commands;

namespace SaaSFoundry.EngineeringWorkbench.Cli;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: dotnet SaaSFoundry.EngineeringWorkbench.Cli.dll <command>");
            return;
        }

        var pluginCommands = new PluginCommands();

        var command = args[0].ToLowerInvariant();
        switch (command)
        {
            case "list":
                await pluginCommands.ListAsync();
                break;
            case "generate":
                var pluginName = args.Length > 1 ? args[1] : "all";
                await pluginCommands.GenerateAsync(pluginName);
                break;
            case "validate":
                await pluginCommands.ValidateAsync();
                break;
            case "report":
                await pluginCommands.ReportAsync();
                break;
            default:
                Console.WriteLine($"Unknown command: {command}");
                break;
        }
    }
}

