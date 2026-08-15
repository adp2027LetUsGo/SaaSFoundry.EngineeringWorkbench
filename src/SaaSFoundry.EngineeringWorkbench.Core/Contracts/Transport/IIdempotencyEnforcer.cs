namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Transport;

public enum IdempotencyAcquisitionStatus
{
    Acquired,
    AlreadyProcessed,
    InProgress
}

public interface IIdempotencyEnforcer
{
    System.Threading.Tasks.Task<IdempotencyAcquisitionStatus> TryAcquireAsync(
        string tenantId,
        string idempotencyKey,
        System.Threading.CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task CompleteAsync(
        string tenantId,
        string idempotencyKey,
        System.Threading.CancellationToken cancellationToken = default);
}
