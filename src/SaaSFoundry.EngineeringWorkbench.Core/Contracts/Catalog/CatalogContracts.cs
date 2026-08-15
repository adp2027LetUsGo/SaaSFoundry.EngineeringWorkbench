using System.Collections.Generic;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Catalog;

public sealed record VersionDescriptor(string Version);

public sealed record ArtifactDescriptor(string Name, string Description);

public sealed record PluginDependency(string PluginId, string MinimumVersion);

public sealed record CapabilityRequirement(string PluginId, string CapabilityId, string Operation);

public sealed record CapabilityManifest(
    string CapabilityId,
    string Operation,
    string Description,
    IReadOnlyList<CapabilityRequirement> Requirements,
    IReadOnlyList<ArtifactDescriptor> ExpectedArtifacts,
    IReadOnlyList<string> ValidationRequirements
);

public sealed record PluginManifest(
    string PluginId,
    string Name,
    VersionDescriptor Version,
    string EngineeringDomain,
    IReadOnlyList<CapabilityManifest> Capabilities,
    IReadOnlyList<PluginDependency> Dependencies,
    string RequiredCanonVersion
);

public sealed record EngineeringPackageManifest(
    string PackageId,
    IReadOnlyList<PluginManifest> Plugins
);

public sealed record CatalogValidationResult(
    bool IsSuccessful,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings
);

public sealed record EngineeringCatalog(
    IReadOnlyList<PluginManifest> RegisteredPlugins,
    CatalogValidationResult ValidationResult
);

