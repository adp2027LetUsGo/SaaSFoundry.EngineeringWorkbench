import os

plugins = [
    ('SaaSFoundry.Plugins.AI', 'AIPlugin', 'ai', 'AI Foundation Plugin'),
    ('SaaSFoundry.Plugins.Import.AI', 'ImportAIPlugin', 'import_ai', 'Import AI Plugin'),
    ('SaaSFoundry.Plugins.Commerce.Shopify', 'ShopifyCommercePlugin', 'commerce_shopify', 'Shopify Commerce Plugin'),
    ('SaaSFoundry.Plugins.ProductIntelligence', 'ProductIntelligencePlugin', 'product_intelligence', 'Product Intelligence Plugin')
]

for proj_dir, plugin_class, plugin_id, plugin_desc in plugins:
    dir_path = os.path.join('src', proj_dir)
    plugin_cs = os.path.join(dir_path, f'{plugin_class}.cs')
    
    # delete all *ExtensionGenerator.cs
    for f in os.listdir(dir_path):
        if f.endswith('ExtensionGenerator.cs'):
            os.remove(os.path.join(dir_path, f))
            
    # generate valid Plugin class
    content = f"""using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.SDK.Plugins.Authoring;

namespace {proj_dir};

public sealed class {plugin_class} : IPluginMetadataProvider
{{
    private readonly IPluginMetadataProvider _innerPlugin;

    public {plugin_class}() : this(null) {{ }}

    public {plugin_class}(IEnumerable<IPluginCapability>? injectedCapabilities)
    {{
        var capabilitiesList = injectedCapabilities != null && injectedCapabilities.Any()
            ? injectedCapabilities.ToArray()
            : Array.Empty<IPluginCapability>();

        _innerPlugin = new PluginBuilder()
            .WithIdentity("{plugin_id}", "1.0.0", "SaaSFoundry Engineering", "SHA256:00000")
            .WithManifest("{plugin_desc}", "{plugin_desc}", "SaaSFoundry.EngineeringWorkbench v1.0", "net10.0", "v1.0.0", "NativeAOT")
            .AddCapabilities(capabilitiesList)
            .Build();
    }}

    public IPluginManifest Manifest => _innerPlugin.Manifest;
    public PluginIdentity Identity => _innerPlugin.Identity;
    public PluginMetadata Metadata => _innerPlugin.Metadata;
    public IReadOnlyCollection<IPluginCapability> Capabilities => _innerPlugin.Capabilities;

    public IPluginCapability? GetCapability(string capabilityId) => _innerPlugin.GetCapability(capabilityId);
    public Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken) => _innerPlugin.InitializeAsync(services, cancellationToken);
    public Task ShutdownAsync(CancellationToken cancellationToken) => _innerPlugin.ShutdownAsync(cancellationToken);
}}
"""
    with open(plugin_cs, 'w', encoding='utf-8') as f:
        f.write(content)
