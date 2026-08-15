using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using SaaSFoundry.SDK.Plugins.Authoring;
using SaaSFoundry.Plugins.Authentication.Capabilities;

namespace SaaSFoundry.Plugins.Authentication.Plugin;

public sealed class AuthenticationPlugin : IPluginMetadataProvider
{
    private readonly IPluginMetadataProvider _innerPlugin;

    public AuthenticationPlugin() : this(null)
    {
    }

    public AuthenticationPlugin(IEnumerable<IPluginCapability>? injectedCapabilities)
    {
        var capabilitiesList = injectedCapabilities != null && injectedCapabilities.Any()
            ? injectedCapabilities.ToArray()
            : CreateDefaultCapabilities();

        _innerPlugin = new PluginBuilder()
            .WithIdentity("authentication", "1.0.0", "SaaSFoundry Engineering", "SHA256:8F3A2B7C1D2E5F0A1B2C3D4E5F6A7B8C9D0E1F2A3B4C5D6E7F8A9B0C1D2E4H")
            .WithManifest("SaaSFoundry Authentication Engineering Plugin", "Production Authentication infrastructure plugin providing dual-scheme authentication (JWT/ApiKey) capabilities without reflection.", "SaaSFoundry.EngineeringWorkbench v1.0", "net10.0", "v1.0.0", "NativeAOT")
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
        return new IPluginCapability[]
        {
            new AuthenticationMiddlewareCapability()
        };
    }
}
