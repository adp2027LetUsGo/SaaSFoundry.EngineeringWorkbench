using System;
using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Catalog;

namespace SaaSFoundry.EngineeringWorkbench.Catalog;

public interface ICatalogManager
{
    EngineeringCatalog BuildCatalog(EngineeringPackageManifest package);
}

public sealed class CatalogManager : ICatalogManager
{
    public EngineeringCatalog BuildCatalog(EngineeringPackageManifest package)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        
        var pluginIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var capabilityIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Duplicate detection
        foreach (var plugin in package.Plugins)
        {
            if (!pluginIds.Add(plugin.PluginId))
            {
                errors.Add($"Duplicate PluginId detected: {plugin.PluginId}");
            }

            foreach (var cap in plugin.Capabilities)
            {
                var fullCapId = $"{plugin.PluginId}.{cap.CapabilityId}.{cap.Operation}";
                if (!capabilityIds.Add(fullCapId))
                {
                    errors.Add($"Duplicate Capability detected: {fullCapId}");
                }
            }
        }

        // 2. Dependency validation
        foreach (var plugin in package.Plugins)
        {
            foreach (var dep in plugin.Dependencies)
            {
                if (!pluginIds.Contains(dep.PluginId))
                {
                    errors.Add($"Missing Plugin Dependency: '{plugin.PluginId}' requires '{dep.PluginId}'");
                }
            }

            foreach (var cap in plugin.Capabilities)
            {
                foreach (var req in cap.Requirements)
                {
                    var reqFullId = $"{req.PluginId}.{req.CapabilityId}.{req.Operation}";
                    if (!capabilityIds.Contains(reqFullId))
                    {
                        errors.Add($"Missing Capability Requirement: '{plugin.PluginId}.{cap.CapabilityId}' requires '{reqFullId}'");
                    }
                }
            }
        }

        var result = new CatalogValidationResult(errors.Count == 0, errors, warnings);
        return new EngineeringCatalog(package.Plugins, result);
    }
}
