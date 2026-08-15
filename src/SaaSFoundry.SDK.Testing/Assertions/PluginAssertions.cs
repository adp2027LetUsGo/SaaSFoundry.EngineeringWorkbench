using System;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

namespace SaaSFoundry.SDK.Testing.Assertions;

public static class PluginAssertions
{
    public static void AssertValid(IEngineeringPlugin? plugin)
    {
        if (plugin == null)
            throw new InvalidOperationException("Plugin is null.");

        if (plugin.Manifest == null)
            throw new InvalidOperationException("Plugin manifest is null.");

        if (string.IsNullOrWhiteSpace(plugin.Manifest.Id))
            throw new InvalidOperationException("Plugin ID is null or empty.");

        if (string.IsNullOrWhiteSpace(plugin.Manifest.Version))
            throw new InvalidOperationException("Plugin Version is null or empty.");

        if (plugin.Capabilities == null)
            throw new InvalidOperationException("Plugin capabilities collection is null.");

        var duplicateCapabilities = plugin.Capabilities
            .GroupBy(c => c.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateCapabilities.Count > 0)
            throw new InvalidOperationException($"Plugin contains duplicate capability IDs: {string.Join(", ", duplicateCapabilities)}");
    }

    public static void AssertHasCapability(IEngineeringPlugin plugin, string capabilityId)
    {
        if (plugin == null)
            throw new InvalidOperationException("Plugin is null.");

        var cap = plugin.GetCapability(capabilityId);
        if (cap == null)
            throw new InvalidOperationException($"Plugin '{plugin.Manifest?.Id}' is missing capability '{capabilityId}'.");
    }
}
