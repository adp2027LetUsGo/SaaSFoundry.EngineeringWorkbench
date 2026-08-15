using System.Threading;
using System.Threading.Tasks;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent.Governance;

public sealed record ApprovalRequest(
    string RequestId,
    string AgentId,
    string CapabilityId,
    AgentRiskLevel RiskLevel,
    string Reason,
    long Timestamp
);

public sealed record ApprovalDecision(
    string RequestId,
    bool IsApproved,
    string ApproverId,
    string? DenialReason,
    long Timestamp
);

public interface IAgentApprovalProvider
{
    Task<ApprovalDecision> RequestApprovalAsync(ApprovalRequest request, CancellationToken cancellationToken = default);
}
