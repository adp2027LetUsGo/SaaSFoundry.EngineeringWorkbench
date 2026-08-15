using System.Collections.Generic;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation;
using SaaSFoundry.SDK.Packaging.Models;
using SaaSFoundry.Plugins.Observability.Traceability;
using SaaSFoundry.SDK.Core.Generators;
using SaaSFoundry.SDK.Core.Diagnostics;

namespace SaaSFoundry.Plugins.Observability.Certification;

public sealed record GoldenReferencePackage(
    string PackageId,
    string PackageHash,
    PluginMetadata PluginMetadata,
    IReadOnlyList<CapabilityGovernanceMetadata> GovernanceMetadata,
    IReadOnlyList<GeneratedArtifactDescriptor> ArtifactInventory,
    ArtifactDependencyGraph DependencyGraph,
    IReadOnlyList<SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation.ValidationEvidence> ValidationEvidence,
    CertifiedPluginDescriptor Certification
);
