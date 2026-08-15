using System;
using System.Collections.Generic;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Packaging;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;

public enum ExecutionStatus
{
    Pending,
    Approved,
    Rejected,
    InProgress,
    Completed,
    Failed
}

public sealed record ExecutionPlanIdentity(
    string PlanId,
    string PlanVersion,
    long CreatedTimestamp,
    string EngineeringPackageId,
    string CanonicalVersion
);

public sealed record ExecutionFingerprint(string Hash);

public sealed record ExecutionApproval(
    string PlanId,
    bool Approved,
    string ApprovalReason,
    long ApprovedTimestamp
);

public sealed record ExecutionContext(
    ExecutionPlanIdentity Identity,
    ExecutionFingerprint Fingerprint,
    ExecutionApproval Approval
);

public sealed record ExecutionEvidence(
    IReadOnlyList<string> ArtifactsGenerated,
    IReadOnlyList<ValidationEvidence> ValidationResults,
    IReadOnlyList<string> ExecutionLogs,
    IReadOnlyList<string> CapabilityResults
);

public sealed record ExecutionRecord(
    string ExecutionId,
    string PlanId,
    ExecutionStatus Status,
    long StartedAt,
    long? CompletedAt,
    IReadOnlyList<string> ExecutedTasks,
    ExecutionEvidence? Evidence
);

public sealed record GovernanceResult(
    bool IsSuccessful,
    string Reason,
    ExecutionContext? Context
);

public interface IExecutionGovernanceEngine
{
    ExecutionFingerprint GenerateFingerprint(ExecutionPlanIdentity identity, string catalogHash, string planHash);
    GovernanceResult ValidateApproval(ExecutionPlanIdentity identity, ExecutionApproval approval, ExecutionFingerprint fingerprint);
}
