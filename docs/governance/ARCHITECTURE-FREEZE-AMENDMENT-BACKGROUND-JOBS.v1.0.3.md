# ARCHITECTURE FREEZE AMENDMENT PROPOSAL: BACKGROUND JOBS (v1.0.3)

## Amendment ID
AMEND-2026-08-01-003

## Status
PROPOSED

## Target
`SaaSFoundry.EngineeringWorkbench.Core`

## Target Freeze Version
Architecture Freeze v1.0.2 -> v1.0.3

## Problem
The previous amendment (v1.0.2) established `IBackgroundJob`, `IBackgroundJobHandler<TJob>`, and `JobExecutionContext` as canonical definitions for job payloads and handlers. However, it lacked the canonical contracts required to represent durable queue state (e.g., JobId, AttemptCount, Status) and the explicit AOT-safe serialization mechanisms necessary to pass arbitrary job payloads between Persistence and BackgroundProcessing. Without these shared canonical abstractions, the Persistence plugin cannot expose a `JobStorageCapability` to BackgroundProcessing without inventing substitute, plugin-local contracts, violating the architecture.

## Proposal

Introduce the following contracts to `SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs`:

### 1. Job Status
```csharp
public enum JobStatus
{
    Queued,
    Started,
    Completed,
    Failed,
    Cancelled
}
```
*Note: `Retried` is an execution event represented by transitioning from `Started` back to `Queued` with an incremented `AttemptCount` and a new `NextExecutionTime`, rather than a distinct persistent status.*

### 2. Failure Information
```csharp
public record JobFailureInformation(
    string Message,
    string? StackTrace,
    DateTimeOffset FailedAt
);
```
*Constraints: Raw Exception objects must NOT be serialized. Stack frames requiring reflection must be avoided. No raw credentials or secrets may be included.*

### 3. Durable Job Envelope
```csharp
public record EnqueuedJob(
    string JobId,
    string JobTypeId,
    string SerializedPayload,
    JobStatus Status,
    int AttemptCount,
    DateTimeOffset? NextExecutionTime,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    JobFailureInformation? FailureInformation,
    JobExecutionContext Context
);
```

### 4. Serialization Contract
```csharp
public interface IJobPayloadSerializer
{
    string Serialize<TJob>(TJob job) where TJob : IBackgroundJob;
    TJob Deserialize<TJob>(string jobTypeId, string serializedPayload) where TJob : IBackgroundJob;
}
```
*Constraint: This explicit interface decouples Core from `System.Text.Json` source generators. The host application will provide an implementation backed by a generated `JsonSerializerContext` covering all known `TJob` implementations. No reflection may be used.*

### 5. Job Storage Capability
```csharp
public interface IJobStorageCapability
{
    Task<string> EnqueueAsync(string jobTypeId, string serializedPayload, JobExecutionContext context, DateTimeOffset? nextExecutionTime, CancellationToken cancellationToken);
    Task<EnqueuedJob?> ClaimNextAsync(CancellationToken cancellationToken);
    Task CompleteAsync(string jobId, CancellationToken cancellationToken);
    Task FailAsync(string jobId, JobFailureInformation failureInfo, CancellationToken cancellationToken);
    Task RetryAsync(string jobId, JobFailureInformation failureInfo, DateTimeOffset nextExecutionTime, CancellationToken cancellationToken);
    Task CancelAsync(string jobId, CancellationToken cancellationToken);
}
```

## Lifecycle Semantics
- **Enqueue**: Initial `Status` = `Queued`, `AttemptCount` = 0. Envelope includes payload and exact `JobExecutionContext`.
- **Claim**: Selects the oldest `Queued` job where `NextExecutionTime <= UtcNow`. Transitions `Status` to `Started`, sets `StartedAt`, increments `AttemptCount`.
- **Complete**: Transitions `Started` to `Completed`, sets `CompletedAt`.
- **Fail**: Transitions `Started` to `Failed`, sets `CompletedAt`, records `FailureInformation`.
- **Retry**: Transitions `Started` to `Queued`, sets `NextExecutionTime`, increments `AttemptCount`.
- **Cancel**: Transitions `Queued` or `Started` to `Cancelled`, sets `CompletedAt`.

## Impact Analysis
- **Core Stability**: Additive only.
- **SDK Stability**: Zero impact. No SDK dependencies introduced.
- **NativeAOT**: Fully compatible. Uses string serialized payloads and avoids dynamic type activation.
- **Persistence Boundary**: `IJobStorageCapability` provides a clean boundary without leaking Npgsql/Dapper types.
- **Security**: Explicitly prohibits serialization of exceptions and credentials. Identity/Tenant/Auth are durably persisted via `JobExecutionContext`.

## Conclusion
This amendment satisfies the requirement for a canonical durable job storage contract, allowing BackgroundProcessing and Persistence to integrate robustly without reflection or non-canonical primitives.
