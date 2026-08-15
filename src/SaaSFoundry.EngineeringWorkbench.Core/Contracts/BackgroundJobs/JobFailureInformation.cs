using System;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;

public record JobFailureInformation(
    string Message,
    string? StackTrace,
    DateTimeOffset FailedAt
);
