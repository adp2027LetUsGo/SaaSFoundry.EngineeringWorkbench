using System.Collections.Generic;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;

public sealed record PluginIdentity(
    string PluginId,
    string Version,
    string Author,
    string Fingerprint
);

public sealed record CapabilityIdentity(
    string PluginId,
    string CapabilityId,
    string Version
);

public sealed record VersionIdentity(
    string SemanticVersion,
    long BuildTimestamp,
    string CompatibilityTarget
);

public sealed record PluginMetadata(
    string Id,
    string Name,
    string Version,
    string Description,
    IReadOnlyList<string> Capabilities,
    IReadOnlyList<string> Dependencies,
    string Author,
    string Compatibility
);

public interface IPluginMetadataProvider : IEngineeringPlugin
{
    PluginIdentity Identity { get; }
    PluginMetadata Metadata { get; }
}
