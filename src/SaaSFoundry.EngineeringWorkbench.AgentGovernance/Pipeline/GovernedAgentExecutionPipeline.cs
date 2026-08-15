using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.AgentGovernance.Engine;
using SaaSFoundry.EngineeringWorkbench.AgentGovernance.Providers;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent.Governance;

namespace SaaSFoundry.EngineeringWorkbench.AgentGovernance.Pipeline;

public sealed class GovernedAgentExecutionPipeline
{
    private readonly AgentGovernanceEngine _engine;
    private readonly IAgentApprovalProvider _approvalProvider;
    private readonly IAgentEventBus? _eventBus;

    public GovernedAgentExecutionPipeline(
        AgentGovernanceEngine? engine = null,
        IAgentApprovalProvider? approvalProvider = null,
        IAgentEventBus? eventBus = null)
    {
        _engine = engine ?? new AgentGovernanceEngine();
        _approvalProvider = approvalProvider ?? new DefaultAgentApprovalProvider(autoApprove: true);
        _eventBus = eventBus;
    }

    public async Task<AgentExecutionResult> ExecuteGovernedCapabilityAsync(
        IAgentOrchestrator agent,
        AgentExecutionContext context,
        string capabilityId,
        IReadOnlyList<string> requestedPermissions,
        Func<CancellationToken, Task<IReadOnlyList<string>>> capabilityAction,
        CancellationToken cancellationToken = default)
    {
        if (agent == null) throw new ArgumentNullException(nameof(agent));
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (string.IsNullOrWhiteSpace(capabilityId)) throw new ArgumentException("CapabilityId cannot be null or empty.", nameof(capabilityId));
        if (capabilityAction == null) throw new ArgumentNullException(nameof(capabilityAction));

        long startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Phase 1: Governance Evaluation & Policy Decision
        var request = new AgentExecutionRequest(
            ExecutionId: context.ExecutionId,
            AgentId: agent.Identity.AgentId,
            CapabilityId: capabilityId,
            Governance: agent.GovernanceMetadata,
            RequestedPermissions: requestedPermissions ?? Array.Empty<string>()
        );

        var decision = await _engine.EvaluateRequestAsync(request, cancellationToken);

        if (decision.IsBlocked || (!decision.CanExecute && !decision.ApprovalRequirement.IsRequired))
        {
            if (_eventBus != null)
            {
                await _eventBus.PublishAsync(new AgentFailedEvent($"evt-fail-{Guid.NewGuid():N}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), agent.Identity.AgentId, decision.PolicyReason), cancellationToken);
            }

            return new AgentExecutionResult(
                ExecutionId: context.ExecutionId,
                AgentId: agent.Identity.AgentId,
                Status: AgentExecutionStatus.Failed,
                FinishedTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                OutputSummary: null,
                GeneratedArtifacts: Array.Empty<string>(),
                ErrorMessage: decision.PolicyReason
            );
        }

        // Phase 2: Approval Gate (if required by High risk or custom policy)
        if (decision.ApprovalRequirement.IsRequired)
        {
            var approvalReq = new ApprovalRequest(
                RequestId: $"appr-{Guid.NewGuid():N}",
                AgentId: agent.Identity.AgentId,
                CapabilityId: capabilityId,
                RiskLevel: agent.GovernanceMetadata.RiskLevel,
                Reason: decision.ApprovalRequirement.Reason ?? "Policy requires explicit approval before execution.",
                Timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            );

            var approvalDecision = await _approvalProvider.RequestApprovalAsync(approvalReq, cancellationToken);

            if (!approvalDecision.IsApproved)
            {
                string denialReason = approvalDecision.DenialReason ?? "Execution denied by approval provider.";
                if (_eventBus != null)
                {
                    await _eventBus.PublishAsync(new AgentFailedEvent($"evt-fail-{Guid.NewGuid():N}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), agent.Identity.AgentId, denialReason), cancellationToken);
                }

                return new AgentExecutionResult(
                    ExecutionId: context.ExecutionId,
                    AgentId: agent.Identity.AgentId,
                    Status: AgentExecutionStatus.Cancelled,
                    FinishedTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    OutputSummary: null,
                    GeneratedArtifacts: Array.Empty<string>(),
                    ErrorMessage: $"Execution denied at approval gate: {denialReason}"
                );
            }
        }

        // Phase 3: Audit Logging if required (Medium/High risk)
        if (decision.RequiresAudit && _eventBus != null)
        {
            await _eventBus.PublishAsync(new AgentExecutionStartedEvent($"evt-exec-{Guid.NewGuid():N}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), agent.Identity.AgentId, capabilityId), cancellationToken);
        }

        // Phase 4: Plugin Execution (Explicit Contract Invocations)
        try
        {
            var artifacts = await capabilityAction(cancellationToken);

            long finishTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (_eventBus != null)
            {
                await _eventBus.PublishAsync(new AgentExecutionCompletedEvent($"evt-comp-{Guid.NewGuid():N}", finishTime, agent.Identity.AgentId, capabilityId, IsSuccess: true), cancellationToken);
            }

            return new AgentExecutionResult(
                ExecutionId: context.ExecutionId,
                AgentId: agent.Identity.AgentId,
                Status: AgentExecutionStatus.Succeeded,
                FinishedTimestamp: finishTime,
                OutputSummary: $"Governed execution of capability '{capabilityId}' completed successfully (Risk Level: {agent.GovernanceMetadata.RiskLevel}).",
                GeneratedArtifacts: artifacts ?? Array.Empty<string>(),
                ErrorMessage: null
            );
        }
        catch (Exception ex)
        {
            long failTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (_eventBus != null)
            {
                await _eventBus.PublishAsync(new AgentFailedEvent($"evt-fail-{Guid.NewGuid():N}", failTime, agent.Identity.AgentId, ex.Message), cancellationToken);
            }

            return new AgentExecutionResult(
                ExecutionId: context.ExecutionId,
                AgentId: agent.Identity.AgentId,
                Status: AgentExecutionStatus.Failed,
                FinishedTimestamp: failTime,
                OutputSummary: null,
                GeneratedArtifacts: Array.Empty<string>(),
                ErrorMessage: $"Exception encountered during capability execution: {ex.Message}"
            );
        }
    }
}
