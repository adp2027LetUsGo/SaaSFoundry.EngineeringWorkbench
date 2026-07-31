using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

namespace SaaSFoundry.EngineeringWorkbench.PluginRuntime.Registration;

public sealed class PluginRegistry
{
    private readonly System.Collections.Generic.Dictionary<string, IEngineeringPlugin> _plugins = new();

    public System.Collections.Generic.IReadOnlyCollection<IEngineeringPlugin> Plugins => _plugins.Values;

    public void RegisterPlugin(IEngineeringPlugin plugin)
    {
        if (!_plugins.TryAdd(plugin.Manifest.Id, plugin))
        {
            throw new System.InvalidOperationException($"Duplicate plugin registration: {plugin.Manifest.Id}");
        }
    }

    public IEngineeringPlugin? GetPlugin(string pluginId)
    {
        _plugins.TryGetValue(pluginId, out var plugin);
        return plugin;
    }
}
