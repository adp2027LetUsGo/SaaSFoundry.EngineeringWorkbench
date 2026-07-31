namespace SaaSFoundry.EngineeringWorkbench.Builder.Services;

public sealed class PluginDiscoveryService
{
    public IReadOnlyList<string> Discover(string pluginsRoot)
    {
        if(!Directory.Exists(pluginsRoot))
            return [];

        return Directory
            .GetDirectories(pluginsRoot)
            .Select(Path.GetFileName)
            .OrderBy(x=>x)
            .ToList();
    }
}
