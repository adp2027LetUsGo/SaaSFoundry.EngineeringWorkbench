using System;
using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.SDK.Plugins.Abstractions;

namespace SaaSFoundry.SDK.Plugins.Authoring;

/// <summary>
/// A deterministic builder for authoring an IEngineeringPlugin implementation.
/// Enforces uniqueness of capabilities, valid identity, and AOT compatibility (zero reflection).
/// </summary>
public sealed class PluginBuilder
{
    private string? _pluginId;
    private string? _name;
    private string? _version;
    private string? _description;
    private string? _author;
    private string[]? _compatibilityTargets;
    private string? _fingerprint;
    
    private readonly List<IPluginCapability> _capabilities = new();
    private readonly List<string> _dependencies = new();

    public PluginBuilder WithIdentity(string pluginId, string version, string author, string fingerprint)
    {
        _pluginId = pluginId;
        _version = version;
        _author = author;
        _fingerprint = fingerprint;
        return this;
    }

    public PluginBuilder WithManifest(string name, string description, params string[] compatibilityTargets)
    {
        _name = name;
        _description = description;
        _compatibilityTargets = compatibilityTargets.Length > 0 ? compatibilityTargets : new[] { "SaaSFoundry.EngineeringWorkbench v1.0" };
        return this;
    }

    public PluginBuilder AddCapability(IPluginCapability capability)
    {
        if (capability == null) throw new ArgumentNullException(nameof(capability));
        
        if (_capabilities.Any(c => string.Equals(c.Id, capability.Id, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Duplicate capability ID detected: '{capability.Id}'. Capability IDs must be unique within a plugin.");
        }
        
        _capabilities.Add(capability);
        return this;
    }

    public PluginBuilder AddCapabilities(params IPluginCapability[] capabilities)
    {
        foreach (var cap in capabilities)
        {
            AddCapability(cap);
        }
        return this;
    }

    public PluginBuilder AddDependency(string dependencyPluginId)
    {
        if (!string.IsNullOrWhiteSpace(dependencyPluginId))
        {
            _dependencies.Add(dependencyPluginId);
        }
        return this;
    }

    public IPluginMetadataProvider Build()
    {
        if (string.IsNullOrWhiteSpace(_pluginId)) throw new InvalidOperationException("PluginId is required.");
        if (string.IsNullOrWhiteSpace(_version)) throw new InvalidOperationException("Version is required.");
        if (string.IsNullOrWhiteSpace(_name)) throw new InvalidOperationException("Name is required.");
        if (_capabilities.Count == 0) throw new InvalidOperationException("At least one capability must be registered.");

        // Internal implementation of the plugin returned by the builder.
        return new SdkEngineeredPlugin(
            _pluginId,
            _name,
            _version,
            _description ?? string.Empty,
            _author ?? string.Empty,
            _fingerprint ?? string.Empty,
            _compatibilityTargets ?? new[] { "SaaSFoundry.EngineeringWorkbench v1.0" },
            _dependencies.ToArray(),
            _capabilities.ToArray()
        );
    }
}
