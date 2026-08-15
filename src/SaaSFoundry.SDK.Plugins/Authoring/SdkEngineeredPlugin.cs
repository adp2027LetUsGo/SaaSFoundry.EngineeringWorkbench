using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

namespace SaaSFoundry.SDK.Plugins.Authoring;

internal sealed class SdkEngineeredPlugin : IPluginMetadataProvider
{
    private readonly Dictionary<string, IPluginCapability> _capabilities;

    public SdkEngineeredPlugin(
        string pluginId,
        string name,
        string version,
        string description,
        string author,
        string fingerprint,
        string[] compatibilityTargets,
        IReadOnlyList<string> dependencies,
        IReadOnlyList<IPluginCapability> capabilities)
    {
        Manifest = new SdkEngineeredPluginManifest(pluginId, name, version, description, dependencies, compatibilityTargets);
        
        Identity = new PluginIdentity(pluginId, version, author, fingerprint);
        
        Metadata = new PluginMetadata(
            pluginId,
            name,
            version,
            description,
            capabilities.Select(c => c.Id).ToArray(),
            dependencies,
            author,
            compatibilityTargets.FirstOrDefault() ?? "SaaSFoundry.EngineeringWorkbench v1.0"
        );

        _capabilities = new Dictionary<string, IPluginCapability>(StringComparer.OrdinalIgnoreCase);
        foreach (var cap in capabilities)
        {
            _capabilities[cap.Id] = cap;
        }
    }

    public IPluginManifest Manifest { get; }

    public PluginIdentity Identity { get; }

    public PluginMetadata Metadata { get; }

    public IReadOnlyCollection<IPluginCapability> Capabilities => _capabilities.Values;

    public IPluginCapability? GetCapability(string capabilityId)
    {
        _capabilities.TryGetValue(capabilityId, out var capability);
        return capability;
    }

    public Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        // Explicit explicit lifecycle support, without enforcing reflection or complex execution logic.
        return Task.CompletedTask;
    }

    public Task ShutdownAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

internal sealed class SdkEngineeredPluginManifest : IPluginManifest
{
    public SdkEngineeredPluginManifest(
        string id, 
        string name, 
        string version, 
        string description, 
        IReadOnlyList<string> dependencies, 
        IReadOnlyList<string> compatibility)
    {
        Id = id;
        Name = name;
        Version = version;
        Description = description;
        Dependencies = dependencies;
        Compatibility = compatibility;
    }

    public string Id { get; }
    public string Name { get; }
    public string Version { get; }
    public string Description { get; }
    public IReadOnlyList<string> Dependencies { get; }
    public IReadOnlyList<string> Compatibility { get; }
}
