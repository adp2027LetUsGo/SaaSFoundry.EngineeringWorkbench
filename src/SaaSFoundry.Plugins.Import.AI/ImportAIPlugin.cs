using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.SDK.Plugins.Authoring;

namespace SaaSFoundry.Plugins.Import.AI;

public sealed class ImportAIPlugin : IPluginMetadataProvider
{
    private readonly IPluginMetadataProvider _innerPlugin;

    public ImportAIPlugin() : this(null) { }

    public ImportAIPlugin(IEnumerable<IPluginCapability>? injectedCapabilities)
    {
        var capabilitiesList = injectedCapabilities != null && injectedCapabilities.Any()
            ? injectedCapabilities.ToArray()
            : Array.Empty<IPluginCapability>();

        _innerPlugin = new PluginBuilder()
            .WithIdentity("import_ai", "1.0.0", "SaaSFoundry Engineering", "SHA256:00000")
            .WithManifest("Import AI Plugin", "Import AI Plugin", "SaaSFoundry.EngineeringWorkbench v1.0", "net10.0", "v1.0.0", "NativeAOT")
            .AddCapabilities(capabilitiesList)
            .Build();
    }

    public IPluginManifest Manifest => _innerPlugin.Manifest;
    public PluginIdentity Identity => _innerPlugin.Identity;
    public PluginMetadata Metadata => _innerPlugin.Metadata;
    public IReadOnlyCollection<IPluginCapability> Capabilities => _innerPlugin.Capabilities;

    public IPluginCapability? GetCapability(string capabilityId) => _innerPlugin.GetCapability(capabilityId);
    public Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken) => _innerPlugin.InitializeAsync(services, cancellationToken);
    public Task ShutdownAsync(CancellationToken cancellationToken) => _innerPlugin.ShutdownAsync(cancellationToken);
}
