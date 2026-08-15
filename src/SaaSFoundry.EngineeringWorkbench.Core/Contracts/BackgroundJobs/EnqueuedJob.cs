using System;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;

public record EnqueuedJob(
    string JobId,
    string JobTypeId,
    string SerializedPayload,
    string SerializedContext,
    JobStatus Status,
    int AttemptCount,
    DateTimeOffset? NextExecutionTime,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    JobFailureInformation? FailureInformation
);
