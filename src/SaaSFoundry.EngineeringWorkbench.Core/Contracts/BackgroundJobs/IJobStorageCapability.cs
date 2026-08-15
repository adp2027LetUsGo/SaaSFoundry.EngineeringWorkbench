using System;
using System.Threading;
using System.Threading.Tasks;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;

public interface IJobStorageCapability
{
    Task<string> EnqueueAsync(string jobTypeId, string serializedPayload, string serializedContext, DateTimeOffset? nextExecutionTime, CancellationToken cancellationToken);
    Task<EnqueuedJob?> ClaimNextAsync(CancellationToken cancellationToken);
    Task CompleteAsync(string jobId, CancellationToken cancellationToken);
    Task FailAsync(string jobId, JobFailureInformation failureInfo, CancellationToken cancellationToken);
    Task RetryAsync(string jobId, JobFailureInformation failureInfo, DateTimeOffset nextExecutionTime, CancellationToken cancellationToken);
    Task CancelAsync(string jobId, CancellationToken cancellationToken);
}
