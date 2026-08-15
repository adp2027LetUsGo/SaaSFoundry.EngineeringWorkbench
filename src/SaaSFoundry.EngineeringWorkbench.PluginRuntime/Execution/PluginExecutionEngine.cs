using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation;
using SaaSFoundry.EngineeringWorkbench.PluginRuntime.Registration;
using System.Collections.Generic;

namespace SaaSFoundry.EngineeringWorkbench.PluginRuntime.Execution;

public sealed record CapabilityExecutionResult(
    IPluginExecutionResult Result,
    IReadOnlyCollection<ValidationEvidence> Evidence,
    IReadOnlyList<string> Artifacts
);

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

    public async System.Threading.Tasks.Task<CapabilityExecutionResult?> ExecuteCapabilityAsync(string pluginId, string capabilityId, IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken)
    {
        var plugin = _registry.GetPlugin(pluginId);
        if (plugin == null) return null;

        var capability = plugin.GetCapability(capabilityId);
        if (capability == null) return null;

        await capability.ValidateConfigurationAsync(cancellationToken);
        await capability.ValidateInputAsync(context, cancellationToken);
        
        var result = await capability.ExecuteAsync(context, cancellationToken);
        
        await capability.GenerateArtifactsAsync(context, cancellationToken);
        
        await capability.ValidateOutputAsync(context, cancellationToken);
        var evidence = await capability.ProduceValidationEvidenceAsync(context, cancellationToken);
        
        var artifacts = capability.ReportGeneratedFiles();

        return new CapabilityExecutionResult(result, evidence, artifacts);
    }

    public async System.Threading.Tasks.Task ShutdownAllAsync(System.Threading.CancellationToken cancellationToken)
    {
        foreach (var plugin in System.Linq.Enumerable.Reverse(_registry.Plugins))
        {
            await plugin.ShutdownAsync(cancellationToken);
        }
    }
}
