using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.EngineeringWorkbench.Plugins.Observability.Capabilities;

namespace SaaSFoundry.EngineeringWorkbench.Plugins.Observability;

public sealed class ObservabilityManifest : IPluginManifest
{
    public string Id => "observability";
    public string Name => "Observability Engineering Plugin";
    public string Version => "1.0.0";
    public string Description => "Implements the SaaS-Foundry observability standards and reference architecture.";
    public System.Collections.Generic.IReadOnlyList<string> Dependencies => System.Array.Empty<string>();
    public System.Collections.Generic.IReadOnlyList<string> Compatibility => new[] { "net10.0", "NativeAOT" };
}

public sealed class ObservabilityPlugin : IEngineeringPlugin
{
    private readonly System.Collections.Generic.Dictionary<string, IPluginCapability> _capabilities;

    public ObservabilityPlugin()
    {
        Manifest = new ObservabilityManifest();
        
        var capabilitiesList = new IPluginCapability[]
        {
            new LoggingCapability(),
            new MetricsCapability(),
            new DistributedTracingCapability(),
            new AuditEvidenceCapability(),
            new TelemetryCapability(),
            new OperationalDashboardsCapability(),
            new ValidationCapability(),
            new DocumentationCapability()
        };

        _capabilities = new System.Collections.Generic.Dictionary<string, IPluginCapability>(System.StringComparer.OrdinalIgnoreCase);
        foreach (var cap in capabilitiesList)
        {
            _capabilities[cap.Id] = cap;
        }
    }

    public IPluginManifest Manifest { get; }
    
    public System.Collections.Generic.IReadOnlyCollection<IPluginCapability> Capabilities => _capabilities.Values;

    public IPluginCapability? GetCapability(string capabilityId)
    {
        _capabilities.TryGetValue(capabilityId, out var capability);
        return capability;
    }

    public System.Threading.Tasks.Task InitializeAsync(System.IServiceProvider services, System.Threading.CancellationToken cancellationToken)
    {
        System.Console.WriteLine("[ObservabilityPlugin] Initialized.");
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public System.Threading.Tasks.Task ShutdownAsync(System.Threading.CancellationToken cancellationToken)
    {
        System.Console.WriteLine("[ObservabilityPlugin] Shutdown.");
        return System.Threading.Tasks.Task.CompletedTask;
    }
}
