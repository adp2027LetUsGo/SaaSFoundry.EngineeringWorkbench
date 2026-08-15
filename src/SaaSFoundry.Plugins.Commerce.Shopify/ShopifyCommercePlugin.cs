using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.SDK.Plugins.Authoring;

namespace SaaSFoundry.Plugins.Commerce.Shopify;

public sealed class ShopifyCommercePlugin : IPluginMetadataProvider
{
    private readonly IPluginMetadataProvider _innerPlugin;

    public ShopifyCommercePlugin() : this(null) { }

    public ShopifyCommercePlugin(IEnumerable<IPluginCapability>? injectedCapabilities)
    {
        var capabilitiesList = injectedCapabilities != null && injectedCapabilities.Any()
            ? injectedCapabilities.ToArray()
            : Array.Empty<IPluginCapability>();

        _innerPlugin = new PluginBuilder()
            .WithIdentity("commerce_shopify", "1.0.0", "SaaSFoundry Engineering", "SHA256:00000")
            .WithManifest("Shopify Commerce Plugin", "Shopify Commerce Plugin", "SaaSFoundry.EngineeringWorkbench v1.0", "net10.0", "v1.0.0", "NativeAOT")
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
