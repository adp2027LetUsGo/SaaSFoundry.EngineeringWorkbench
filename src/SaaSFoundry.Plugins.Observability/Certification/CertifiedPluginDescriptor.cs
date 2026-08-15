using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;

namespace SaaSFoundry.Plugins.Observability.Certification;

public sealed record CertifiedPluginDescriptor(
    PluginIdentity PluginIdentity,
    PluginMetadata PluginMetadata,
    int CapabilityCount,
    int ArtifactCount,
    double TraceabilityCoverage,
    double GovernanceCoverage,
    string ValidationStatus,
    long CertificationTimestamp,
    string CertificationHash
);
