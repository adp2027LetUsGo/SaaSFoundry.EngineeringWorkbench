using System;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;

namespace SaaSFoundry.SDK.Testing.Assertions;

public static class GovernanceAssertions
{
    public static void AssertGoverned(IPluginCapability? capability)
    {
        if (capability == null)
            throw new InvalidOperationException("Capability is null.");

        if (capability is not IGovernedPluginCapability governed)
            throw new InvalidOperationException($"Capability '{capability.Id}' does not implement IGovernedPluginCapability.");

        if (governed.GovernanceMetadata == null)
            throw new InvalidOperationException($"Capability '{capability.Id}' returned null GovernanceMetadata.");

        if (string.IsNullOrWhiteSpace(governed.GovernanceMetadata.CapabilityId))
            throw new InvalidOperationException($"Governance metadata for '{capability.Id}' has missing CapabilityId.");

        if (string.IsNullOrWhiteSpace(governed.GovernanceMetadata.OperationType))
            throw new InvalidOperationException($"Governance metadata for '{capability.Id}' has missing OperationType.");

        if (!Enum.IsDefined(typeof(RiskLevel), governed.GovernanceMetadata.Risk))
            throw new InvalidOperationException($"Governance metadata for '{capability.Id}' has invalid RiskLevel.");
    }
}
