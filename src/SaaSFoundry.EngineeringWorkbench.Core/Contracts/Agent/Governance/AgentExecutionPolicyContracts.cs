using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent.Governance;

public sealed record AgentExecutionRequest(
    string ExecutionId,
    string AgentId,
    string CapabilityId,
    AgentGovernanceMetadata Governance,
    IReadOnlyList<string> RequestedPermissions
);

public sealed record AgentPolicyViolation(
    string ViolationId,
    string Description,
    string Severity
);

public sealed record AgentApprovalRequirement(
    bool IsRequired,
    string? Reason,
    string? RequiredApproverRole
);

public sealed record AgentExecutionDecision(
    bool CanExecute,
    bool IsBlocked,
    bool RequiresAudit,
    AgentApprovalRequirement ApprovalRequirement,
    IReadOnlyList<AgentPolicyViolation> Violations,
    string PolicyReason
);

public interface IAgentExecutionPolicy
{
    Task<AgentExecutionDecision> EvaluateAsync(AgentExecutionRequest request, CancellationToken cancellationToken = default);
}
