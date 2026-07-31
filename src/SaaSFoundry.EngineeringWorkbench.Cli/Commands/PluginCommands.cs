namespace SaaSFoundry.EngineeringWorkbench.Cli.Commands;

public sealed class PluginCommands
{
    private const string WorkbenchRoot =
        @"C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench";

    private readonly string _pluginsRoot =
        Path.Combine(
            WorkbenchRoot,
            "plugins");


    public Task ListAsync()
    {
        if(!Directory.Exists(_pluginsRoot))
        {
            Console.WriteLine(
                $"Plugins folder not found: {_pluginsRoot}");

            return Task.CompletedTask;
        }


        Console.WriteLine("Plugins:");
        Console.WriteLine();


        foreach(var plugin in Directory.GetDirectories(_pluginsRoot))
        {
            Console.WriteLine(
                $" - {Path.GetFileName(plugin)}");
        }

        return Task.CompletedTask;
    }


    public Task GenerateAsync(string plugin)
    {
        Console.WriteLine(
            $"Generating plugin: {plugin}");

        return Task.CompletedTask;
    }


    public Task ValidateAsync()
    {
        Console.WriteLine(
            "Validation completed.");

        return Task.CompletedTask;
    }


    public Task ReportAsync()
    {
        Console.WriteLine(
            "Report generation completed.");

        return Task.CompletedTask;
    }
}
