using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.Plugins.Persistence.Capabilities.Connection;
using SaaSFoundry.Plugins.Persistence.Capabilities.Query;
using SaaSFoundry.SDK.Plugins.Authoring;

namespace SaaSFoundry.Plugins.Persistence.Plugin;

public sealed class PersistencePlugin : IPluginMetadataProvider
{
    private readonly IPluginMetadataProvider _innerPlugin;

    public PersistencePlugin() : this(null)
    {
    }

    public PersistencePlugin(IEnumerable<IPluginCapability>? injectedCapabilities)
    {
        var capabilitiesList = injectedCapabilities != null && injectedCapabilities.Any()
            ? injectedCapabilities.ToArray()
            : CreateDefaultCapabilities();

        _innerPlugin = new PluginBuilder()
            .WithIdentity("persistence", "1.0.0", "SaaSFoundry Engineering", "SHA256:5B1E8B7F1C2D3E5F0A1B2C3D4E5F6A7B8C9D0E1F2A3B4C5D6E7F8A9B0C1D2E4G")
            .WithManifest("SaaSFoundry Persistence Engineering Plugin", "Production persistence plugin implementing Database-per-Cell multitenancy with PostgreSQL, Npgsql, and Dapper.AOT.", "SaaSFoundry.EngineeringWorkbench v1.0", "net10.0", "v1.0.0", "NativeAOT")
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
            new ConnectionCapability(),
            new QueryCapability(),
            new SaaSFoundry.Plugins.Persistence.Capabilities.JobStorage.JobStorageCapability(),
            new SaaSFoundry.Plugins.Persistence.Capabilities.Idempotency.IdempotencyCapability()
        };
    }
}
