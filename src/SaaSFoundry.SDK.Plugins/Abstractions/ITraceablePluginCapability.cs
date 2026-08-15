using System.Collections.Generic;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;
using SaaSFoundry.SDK.Core.Generators;

namespace SaaSFoundry.SDK.Plugins.Abstractions;

/// <summary>
/// Represents a governed capability that generates deterministic artifact descriptors with full engineering traceability.
/// </summary>
public interface ITraceablePluginCapability : IGovernedPluginCapability
{
    /// <summary>
    /// The canonical reference identifier for the architecture document this capability implements.
    /// </summary>
    string CanonReference { get; }

    /// <summary>
    /// The specific engineering implementation reference for this capability.
    /// </summary>
    string ImplementationReference { get; }

    /// <summary>
    /// Returns the deterministic artifact descriptors for this capability without performing filesystem operations.
    /// </summary>
    IReadOnlyList<GeneratedArtifactDescriptor> GetArtifactDescriptors();
}
