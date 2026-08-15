using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;

public enum AgentExecutionStatus
{
    Pending = 0,
    InProgress = 1,
    Succeeded = 2,
    Failed = 3,
    Cancelled = 4
}

public sealed record AgentCapabilityMetadata(
    string CapabilityId,
    string Description,
    string ExpectedInput,
    string ExpectedOutput,
    IReadOnlyList<string> RequiredPermissions
);

public sealed record AgentExecutionContext(
    string ExecutionId,
    string AgentId,
    string Goal,
    long StartedTimestamp,
    IReadOnlyDictionary<string, string> Parameters
);

public sealed record AgentExecutionResult(
    string ExecutionId,
    string AgentId,
    AgentExecutionStatus Status,
    long FinishedTimestamp,
    string? OutputSummary,
    IReadOnlyList<string> GeneratedArtifacts,
    string? ErrorMessage
);

public interface IAgentOrchestrator : IAgentMetadataProvider, IAgentGovernedComponent
{
    Task<AgentExecutionResult> ExecuteAsync(AgentExecutionContext context, CancellationToken cancellationToken = default);
}
