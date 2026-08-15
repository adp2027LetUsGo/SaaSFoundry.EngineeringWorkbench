#nullable enable

using System;
using System.Collections.Generic;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;

namespace SaaSFoundry.EngineeringWorkbench.Execution.Capabilities;

/// <summary>
/// Immutable descriptor defining an agent's operational capability and execution requirements.
/// </summary>
/// <param name="AgentId">Unique ID of the providing agent.</param>
/// <param name="CapabilityId">Unique identifier of the capability.</param>
/// <param name="CapabilityName">Human-readable descriptive capability title.</param>
/// <param name="RiskLevel">Governance operational risk classification.</param>
/// <param name="EstimatedDurationMilliseconds">Approximate execution runtime duration in milliseconds.</param>
/// <param name="Dependencies">Required prerequisite capability or artifact tokens.</param>
/// <param name="RequiredPermissions">System permissions required for execution.</param>
/// <param name="ProducesArtifacts">Output artifact types or filenames generated.</param>
/// <param name="ConsumesArtifacts">Input artifact types consumed by this capability.</param>
/// <param name="SupportsRetry">Whether execution failure can be cleanly retried.</param>
/// <param name="SupportsParallelExecution">Whether this capability supports safe concurrent execution.</param>
/// <param name="SupportsApproval">Whether execution can block for governance or human approval.</param>
/// <param name="SupportsCheckpointing">Whether execution state can be checkpointed and resumed.</param>
public sealed record AgentCapabilityDescriptor(
    string AgentId,
    string CapabilityId,
    string CapabilityName,
    AgentRiskLevel RiskLevel,
    long EstimatedDurationMilliseconds,
    IReadOnlyList<string> Dependencies,
    IReadOnlyList<string> RequiredPermissions,
    IReadOnlyList<string> ProducesArtifacts,
    IReadOnlyList<string> ConsumesArtifacts,
    bool SupportsRetry,
    bool SupportsParallelExecution,
    bool SupportsApproval,
    bool SupportsCheckpointing
);
