using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;

public enum RiskLevel
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public sealed record CapabilityGovernanceMetadata(
    string CapabilityId,
    string OperationType,
    IReadOnlyList<string> RequiredPermissions,
    IReadOnlyList<string> ValidationRequirements,
    RiskLevel Risk
);

public interface IGovernedPluginCapability : IPluginCapability
{
    CapabilityGovernanceMetadata GovernanceMetadata { get; }
}

public sealed record PolicyDecision(
    bool CanExecutePlugin,
    bool CanExecuteCapability,
    bool IsApprovalRequired,
    bool IsValidationMandatory,
    string Reason
);

public interface IPluginExecutionPolicy
{
    Task<PolicyDecision> EvaluatePluginAsync(IEngineeringPlugin plugin, CancellationToken cancellationToken = default);
}

public interface ICapabilityExecutionPolicy
{
    Task<PolicyDecision> EvaluateCapabilityAsync(IEngineeringPlugin plugin, IPluginCapability capability, CancellationToken cancellationToken = default);
}
