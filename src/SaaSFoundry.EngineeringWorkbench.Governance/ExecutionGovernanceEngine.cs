using System;
using System.Security.Cryptography;
using System.Text;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;

namespace SaaSFoundry.EngineeringWorkbench.Governance;

public sealed class ExecutionGovernanceEngine : IExecutionGovernanceEngine
{
    public ExecutionFingerprint GenerateFingerprint(ExecutionPlanIdentity identity, string catalogHash, string planHash)
    {
        var rawData = $"{identity.PlanId}:{identity.EngineeringPackageId}:{catalogHash}:{planHash}";
        
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        
        var hashBuilder = new StringBuilder();
        foreach(var b in bytes)
        {
            hashBuilder.Append(b.ToString("x2"));
        }
        
        return new ExecutionFingerprint(hashBuilder.ToString());
    }

    public GovernanceResult ValidateApproval(ExecutionPlanIdentity identity, ExecutionApproval approval, ExecutionFingerprint fingerprint)
    {
        if (approval == null)
        {
            return new GovernanceResult(false, "Execution is not approved (Missing approval).", null);
        }

        if (!approval.Approved)
        {
            return new GovernanceResult(false, $"Execution is explicitly rejected. Reason: {approval.ApprovalReason}", null);
        }

        if (approval.PlanId != identity.PlanId)
        {
            return new GovernanceResult(false, "Approval PlanId does not match ExecutionPlanIdentity.", null);
        }

        var context = new SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance.ExecutionContext(identity, fingerprint, approval);
        return new GovernanceResult(true, "Execution Approved.", context);
    }
}
