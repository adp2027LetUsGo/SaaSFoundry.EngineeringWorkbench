using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent.Governance;

namespace SaaSFoundry.EngineeringWorkbench.AgentGovernance.Policy;

public sealed class StandardAgentExecutionPolicy : IAgentExecutionPolicy
{
    public Task<AgentExecutionDecision> EvaluateAsync(AgentExecutionRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (request.Governance == null) throw new ArgumentException("Governance metadata cannot be null.", nameof(request));

        var violations = new List<AgentPolicyViolation>();

        // 1. Verify permissions: Requested permissions must not exceed the capabilities granted in GovernanceMetadata
        if (request.RequestedPermissions != null)
        {
            foreach (var perm in request.RequestedPermissions)
            {
                if (request.Governance.RequiredPermissions == null ||
                    !request.Governance.RequiredPermissions.Contains(perm, StringComparer.OrdinalIgnoreCase))
                {
                    violations.Add(new AgentPolicyViolation(
                        ViolationId: $"ERR-PERM-{Guid.NewGuid():N}",
                        Description: $"Requested permission '{perm}' is not authorized by the agent's governance metadata.",
                        Severity: "High"
                    ));
                }
            }
        }

        // 2. Verify capability whitelist if specified
        if (request.Governance.AllowedCapabilities != null && request.Governance.AllowedCapabilities.Count > 0)
        {
            if (!request.Governance.AllowedCapabilities.Contains(request.CapabilityId, StringComparer.OrdinalIgnoreCase))
            {
                violations.Add(new AgentPolicyViolation(
                    ViolationId: $"ERR-CAP-{Guid.NewGuid():N}",
                    Description: $"Capability '{request.CapabilityId}' is not included in the agent's allowed capabilities list.",
                    Severity: "Critical"
                ));
            }
        }

        if (violations.Count > 0)
        {
            return Task.FromResult(new AgentExecutionDecision(
                CanExecute: false,
                IsBlocked: true,
                RequiresAudit: true,
                ApprovalRequirement: new AgentApprovalRequirement(IsRequired: false, Reason: "Blocked due to governance violations.", RequiredApproverRole: null),
                Violations: violations.AsReadOnly(),
                PolicyReason: $"Execution blocked: {violations.Count} policy violation(s) encountered."
            ));
        }

        // 3. Evaluate deterministic risk rules
        return request.Governance.RiskLevel switch
        {
            AgentRiskLevel.None or AgentRiskLevel.Low => Task.FromResult(new AgentExecutionDecision(
                CanExecute: true,
                IsBlocked: false,
                RequiresAudit: false,
                ApprovalRequirement: new AgentApprovalRequirement(IsRequired: false, Reason: null, RequiredApproverRole: null),
                Violations: Array.Empty<AgentPolicyViolation>(),
                PolicyReason: $"Automatic approval granted for risk level '{request.Governance.RiskLevel}'."
            )),

            AgentRiskLevel.Medium => Task.FromResult(new AgentExecutionDecision(
                CanExecute: true,
                IsBlocked: false,
                RequiresAudit: true,
                ApprovalRequirement: new AgentApprovalRequirement(IsRequired: false, Reason: null, RequiredApproverRole: null),
                Violations: Array.Empty<AgentPolicyViolation>(),
                PolicyReason: "Approved with mandatory execution audit requirement for Medium risk level."
            )),

            AgentRiskLevel.High => Task.FromResult(new AgentExecutionDecision(
                CanExecute: false,
                IsBlocked: false,
                RequiresAudit: true,
                ApprovalRequirement: new AgentApprovalRequirement(IsRequired: true, Reason: "High risk agent execution requires explicit human-in-the-loop approval.", RequiredApproverRole: "EngineeringManager"),
                Violations: Array.Empty<AgentPolicyViolation>(),
                PolicyReason: "Execution paused: Explicit human approval required for High risk level."
            )),

            AgentRiskLevel.Critical or _ => Task.FromResult(new AgentExecutionDecision(
                CanExecute: false,
                IsBlocked: true,
                RequiresAudit: true,
                ApprovalRequirement: new AgentApprovalRequirement(IsRequired: false, Reason: "Critical risk execution is blocked by canonical policy.", RequiredApproverRole: null),
                Violations: new[] { new AgentPolicyViolation("ERR-CRIT-001", "Execution of Critical risk agent capabilities is prohibited.", "Critical") },
                PolicyReason: "Execution blocked: Critical risk level exceeds allowable thresholds."
            ))
        };
    }
}
