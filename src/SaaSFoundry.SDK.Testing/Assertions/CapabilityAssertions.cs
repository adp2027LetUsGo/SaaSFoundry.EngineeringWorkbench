using System;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.SDK.Plugins.Abstractions;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;

namespace SaaSFoundry.SDK.Testing.Assertions;

public static class CapabilityAssertions
{
    public static void AssertTraceable(IPluginCapability? capability)
    {
        if (capability == null)
            throw new InvalidOperationException("Capability is null.");

        if (capability is not ITraceablePluginCapability traceable)
            throw new InvalidOperationException($"Capability '{capability.Id}' does not implement ITraceablePluginCapability.");

        if (string.IsNullOrWhiteSpace(traceable.Id))
            throw new InvalidOperationException("Traceable capability has null or empty ID.");

        if (string.IsNullOrWhiteSpace(traceable.CanonReference))
            throw new InvalidOperationException($"Capability '{traceable.Id}' has null or empty CanonReference.");

        if (string.IsNullOrWhiteSpace(traceable.ImplementationReference))
            throw new InvalidOperationException($"Capability '{traceable.Id}' has null or empty ImplementationReference.");
            
        var artifacts = traceable.GetArtifactDescriptors();
        if (artifacts == null)
            throw new InvalidOperationException($"Capability '{traceable.Id}' returned null for artifact descriptors.");
    }
}
