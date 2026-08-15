using System.Collections.Generic;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;

public enum AgentRiskLevel
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public sealed record AgentGovernanceMetadata(
    string AgentId,
    AgentRiskLevel RiskLevel,
    IReadOnlyList<string> RequiredPermissions,
    IReadOnlyList<string> AllowedCapabilities,
    IReadOnlyList<string> ValidationRequirements
);

public interface IAgentGovernedComponent
{
    AgentGovernanceMetadata GovernanceMetadata { get; }
}
