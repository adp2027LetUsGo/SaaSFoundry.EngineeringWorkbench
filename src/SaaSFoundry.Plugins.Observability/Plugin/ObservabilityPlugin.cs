using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.Plugins.Observability.Capabilities.Alerts;
using SaaSFoundry.Plugins.Observability.Capabilities.Collector;
using SaaSFoundry.Plugins.Observability.Capabilities.Configuration;
using SaaSFoundry.Plugins.Observability.Capabilities.Dashboards;
using SaaSFoundry.Plugins.Observability.Capabilities.Documentation;
using SaaSFoundry.Plugins.Observability.Capabilities.HealthChecks;
using SaaSFoundry.Plugins.Observability.Capabilities.Logging;
using SaaSFoundry.Plugins.Observability.Capabilities.Metrics;
using SaaSFoundry.Plugins.Observability.Capabilities.Tracing;
using SaaSFoundry.Plugins.Observability.Capabilities.Validation;
using SaaSFoundry.SDK.Plugins.Authoring;

namespace SaaSFoundry.Plugins.Observability.Plugin;

public sealed class ObservabilityPlugin : IPluginMetadataProvider
{
    private readonly IPluginMetadataProvider _innerPlugin;

    public ObservabilityPlugin(IEnumerable<IPluginCapability>? injectedCapabilities = null)
    {
        var capabilitiesList = injectedCapabilities != null && injectedCapabilities.Any()
            ? injectedCapabilities.ToArray()
            : CreateDefaultCapabilities();

        _innerPlugin = new PluginBuilder()
            .WithIdentity("observability", "1.0.0", "SaaSFoundry Engineering", "SHA256:4A9E8B7F1C2D3E5F0A1B2C3D4E5F6A7B8C9D0E1F2A3B4C5D6E7F8A9B0C1D2E3F")
            .WithManifest("SaaSFoundry Observability Engineering Plugin", "Production observability plugin implementing canonical trace and metrics architectures.", "SaaSFoundry.EngineeringWorkbench v1.0", "net10.0", "v1.0.0", "NativeAOT")
            .AddCapabilities(capabilitiesList)
            .Build();
    }

    public IPluginManifest Manifest => _innerPlugin.Manifest;

    public PluginIdentity Identity => _innerPlugin.Identity;

    public PluginMetadata Metadata => _innerPlugin.Metadata;

    public IReadOnlyCollection<IPluginCapability> Capabilities => _innerPlugin.Capabilities;

    public IPluginCapability? GetCapability(string capabilityId)
    {
        return _innerPlugin.GetCapability(capabilityId);
    }

    public Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        return _innerPlugin.InitializeAsync(services, cancellationToken);
    }

    public Task ShutdownAsync(CancellationToken cancellationToken)
    {
        return _innerPlugin.ShutdownAsync(cancellationToken);
    }

    private static IPluginCapability[] CreateDefaultCapabilities()
    {
        // Explicitly instantiated without runtime reflection for full NativeAOT compatibility
        return new IPluginCapability[]
        {
            new LoggingCapability(),
            new MetricsCapability(),
            new TracingCapability(),
            new HealthChecksCapability(),
            new CollectorCapability(),
            new ConfigurationCapability(),
            new DashboardsCapability(),
            new AlertsCapability(),
            new DocumentationCapability(),
            new ValidationCapability()
        };
    }
}
