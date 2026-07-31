using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.EngineeringWorkbench.PluginRuntime.Registration;

namespace SaaSFoundry.EngineeringWorkbench.PluginRuntime.Execution;

public sealed class PluginExecutionEngine
{
    private readonly PluginRegistry _registry;
    private readonly System.IServiceProvider _services;

    public PluginExecutionEngine(PluginRegistry registry, System.IServiceProvider services)
    {
        _registry = registry;
        _services = services;
    }

    public async System.Threading.Tasks.Task InitializeAllAsync(System.Threading.CancellationToken cancellationToken)
    {
        foreach (var plugin in _registry.Plugins)
        {
            await plugin.InitializeAsync(_services, cancellationToken);
        }
    }

    public async System.Threading.Tasks.Task<IPluginExecutionResult?> ExecutePluginAsync(string pluginId, IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken)
    {
        var plugin = _registry.GetPlugin(pluginId);
        if (plugin == null) return null;

        await plugin.Validator.ValidateConfigurationAsync(cancellationToken);
        await plugin.Validator.ValidateInputAsync(cancellationToken);
        
        var result = await plugin.Executor.ExecuteCapabilityAsync(context, cancellationToken);
        
        await plugin.ArtifactGenerator.GenerateArtifactsAsync(cancellationToken);
        
        await plugin.Validator.ValidateOutputAsync(cancellationToken);
        await plugin.Validator.ProduceValidationEvidenceAsync(cancellationToken);

        return result;
    }

    public async System.Threading.Tasks.Task ShutdownAllAsync(System.Threading.CancellationToken cancellationToken)
    {
        foreach (var plugin in System.Linq.Enumerable.Reverse(_registry.Plugins))
        {
            await plugin.ShutdownAsync(cancellationToken);
        }
    }
}
