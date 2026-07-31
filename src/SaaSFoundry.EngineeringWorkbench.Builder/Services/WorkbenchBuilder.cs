using SaaSFoundry.EngineeringWorkbench.Core.Contracts;

namespace SaaSFoundry.EngineeringWorkbench.Builder.Services;

public sealed class WorkbenchBuilder
    : IWorkbenchBuilder
{
    public Task ListPluginsAsync()
    {
        var root=
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "plugins");

        var service=
            new PluginDiscoveryService();

        var plugins=
            service.Discover(root);

        Console.WriteLine();

        Console.WriteLine("Installed plugins");

        Console.WriteLine("-----------------");

        foreach(var plugin in plugins)
            Console.WriteLine(plugin);

        Console.WriteLine();

        return Task.CompletedTask;
    }

    public Task GeneratePluginAsync(string pluginName)
        => Task.CompletedTask;

    public Task ValidatePluginAsync(string pluginName)
        => Task.CompletedTask;

    public Task PackagePluginAsync(string pluginName)
        => Task.CompletedTask;

    public Task ReportPluginAsync(string pluginName)
        => Task.CompletedTask;
}
