using System;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

namespace SaaSFoundry.SDK.Testing.Assertions;

public static class ManifestAssertions
{
    public static void AssertCompatible(IPluginManifest? manifest, string requiredTarget)
    {
        if (manifest == null)
            throw new InvalidOperationException("Manifest is null.");

        if (manifest.Compatibility == null || manifest.Compatibility.Count == 0)
            throw new InvalidOperationException("Manifest has no compatibility targets.");

        if (!manifest.Compatibility.Contains(requiredTarget))
            throw new InvalidOperationException($"Manifest compatibility targets do not contain the required target '{requiredTarget}'.");
    }

    public static void AssertExactMatch(IPluginManifest? manifest, string expectedId, string expectedVersion)
    {
        if (manifest == null)
            throw new InvalidOperationException("Manifest is null.");

        if (!string.Equals(manifest.Id, expectedId, StringComparison.Ordinal))
            throw new InvalidOperationException($"Manifest ID mismatch. Expected '{expectedId}', but got '{manifest.Id}'.");

        if (!string.Equals(manifest.Version, expectedVersion, StringComparison.Ordinal))
            throw new InvalidOperationException($"Manifest Version mismatch. Expected '{expectedVersion}', but got '{manifest.Version}'.");
    }
}
