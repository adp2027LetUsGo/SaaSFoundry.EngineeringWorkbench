using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

namespace SaaSFoundry.EngineeringWorkbench.Governance.Policies;

public sealed class StandardGovernancePolicy : IPluginExecutionPolicy, ICapabilityExecutionPolicy
{
    public Task<PolicyDecision> EvaluatePluginAsync(IEngineeringPlugin plugin, CancellationToken cancellationToken = default)
    {
        if (plugin == null || plugin.Manifest == null)
        {
            return Task.FromResult(new PolicyDecision(false, false, true, true, "Invalid plugin or missing manifest."));
        }

        return Task.FromResult(new PolicyDecision(
            CanExecutePlugin: true,
            CanExecuteCapability: true,
            IsApprovalRequired: false,
            IsValidationMandatory: true,
            Reason: $"Plugin '{plugin.Manifest.Id}' is allowed under standard deterministic governance."
        ));
    }

    public Task<PolicyDecision> EvaluateCapabilityAsync(IEngineeringPlugin plugin, IPluginCapability capability, CancellationToken cancellationToken = default)
    {
        if (capability == null)
        {
            return Task.FromResult(new PolicyDecision(true, false, true, true, "Invalid capability."));
        }

        if (capability is IGovernedPluginCapability governed && governed.GovernanceMetadata != null)
        {
            var meta = governed.GovernanceMetadata;
            bool isApprovalRequired = meta.Risk >= RiskLevel.Medium;
            bool isValidationMandatory = meta.ValidationRequirements != null && meta.ValidationRequirements.Any();

            string reason = isApprovalRequired
                ? $"Capability '{meta.CapabilityId}' operates at '{meta.Risk}' risk level; explicit approval required."
                : $"Capability '{meta.CapabilityId}' operates at '{meta.Risk}' risk level; allowed without explicit capability-level sign-off.";

            return Task.FromResult(new PolicyDecision(
                CanExecutePlugin: true,
                CanExecuteCapability: true,
                IsApprovalRequired: isApprovalRequired,
                IsValidationMandatory: isValidationMandatory,
                Reason: reason
            ));
        }

        // Default evaluation for standard capabilities
        return Task.FromResult(new PolicyDecision(
            CanExecutePlugin: true,
            CanExecuteCapability: true,
            IsApprovalRequired: false,
            IsValidationMandatory: true,
            Reason: $"Capability '{capability.Id}' evaluated with default standard policy."
        ));
    }
}
