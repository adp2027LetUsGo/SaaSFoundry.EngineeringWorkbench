using System;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent.Governance;

namespace SaaSFoundry.EngineeringWorkbench.AgentGovernance.Providers;

public sealed class DefaultAgentApprovalProvider : IAgentApprovalProvider
{
    private readonly bool _autoApprove;
    private readonly string _approverId;

    public DefaultAgentApprovalProvider(bool autoApprove = true, string approverId = "System-Automated-Gate")
    {
        _autoApprove = autoApprove;
        _approverId = approverId ?? throw new ArgumentNullException(nameof(approverId));
    }

    public Task<ApprovalDecision> RequestApprovalAsync(ApprovalRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (_autoApprove)
        {
            return Task.FromResult(new ApprovalDecision(
                RequestId: request.RequestId,
                IsApproved: true,
                ApproverId: _approverId,
                DenialReason: null,
                Timestamp: timestamp
            ));
        }

        return Task.FromResult(new ApprovalDecision(
            RequestId: request.RequestId,
            IsApproved: false,
            ApproverId: _approverId,
            DenialReason: "Approval rejected by deterministic provider configuration.",
            Timestamp: timestamp
        ));
    }
}
