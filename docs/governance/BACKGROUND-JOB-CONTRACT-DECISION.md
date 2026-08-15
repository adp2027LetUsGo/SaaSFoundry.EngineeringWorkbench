# BACKGROUND JOB CONTRACT DECISION

## Decision ID
DEC-2026-08-01-003

## Problem
BackgroundProcessing requires canonical job contracts to represent a Job Payload and its Execution Context (containing Tenant, Identity, and Authorization constraints). It also requires durable execution state (JobEnvelope) and an AOT-safe serialization interface. These contracts must decouple the plugin's internal mechanism (PostgreSQL-backed static dispatcher) from the application workloads and from the Persistence capability implementation.

## Canonical Ownership
Following the precedent of the Identity/Tenancy architectural amendment, all cross-cutting Platform runtime contracts must reside in the Golden Reference: `SaaSFoundry.EngineeringWorkbench.Core`.

Therefore, BackgroundProcessing MUST NOT define its own proprietary `IJob`, `JobExecutionContext`, or `JobEnvelope` primitives locally. 

## Minimum Contract Set (v1.0.2 + v1.0.3)
The following minimum contracts are required to support a deterministic, AOT-compatible static dispatch model, Tenant/Identity propagation, and durable persistent queues:

| Concept | Owner | Canonical Type | Members | Evidence | AOT Impact |
| :--- | :--- | :--- | :--- | :--- | :--- |
| Job Payload | Core | `IBackgroundJob` | `JobTypeId` | Required for static string-based dispatch (Stage 9C) | Zero reflection |
| Execution Context | Core | `JobExecutionContext` | `Identity, Tenant, Authorization` | Required for Tenant/Identity propagation (Stage 9B) | Structurally AOT compatible |
| Handler | Core | `IBackgroundJobHandler<TJob>` | `ExecuteAsync(TJob, context, token)` | Required for async execution | Strongly typed, static dispatch |
| Durable Job Envelope | Core | `EnqueuedJob` | `JobId, JobTypeId, SerializedPayload, Status, AttemptCount, NextExecutionTime, FailureInformation, Context` | Represents the queue state agnostic of the queue transport. Holds `JobExecutionContext`. | Plain data record, AOT compatible |
| Job Status | Core | `JobStatus` (enum) | `Queued, Started, Completed, Failed, Cancelled` | Canonical lifecycle states (Stage 9G.2). `Retried` is represented by `Queued` with `AttemptCount > 0`. | Zero impact |
| Failure Information | Core | `JobFailureInformation` | `Message, StackTrace, FailedAt` | Isolates error state without requiring arbitrary Exception serialization. | Prohibits dynamic exception serialization |
| Payload Serialization | Core | `IJobPayloadSerializer` | `Serialize<TJob>`, `Deserialize<TJob>` | Required because NativeAOT prohibits `System.Text.Json` from reflecting over unknown payloads. | Explicit registry mapping required |
| Job Storage | Core | `IJobStorageCapability` | `EnqueueAsync, ClaimNextAsync, CompleteAsync, FailAsync, RetryAsync, CancelAsync` | Decouples the generic queue storage from Persistence internals. | Zero reflection |
| Claim | Persistence | `IJobStorageCapability.ClaimNextAsync` | Returns `EnqueuedJob` | Required to fetch oldest `Queued` job, update to `Started`. SQL `FOR UPDATE SKIP LOCKED` hidden internally. | Zero reflection |
| Enqueue | Persistence | `IJobStorageCapability.EnqueueAsync` | Returns `string` JobId | Requires `JobTypeId`, `SerializedPayload`, `JobExecutionContext`. | Zero reflection |
| Complete | Persistence | `IJobStorageCapability.CompleteAsync` | Transition to `Completed` | Finalizes job execution. | Zero reflection |
| Fail | Persistence | `IJobStorageCapability.FailAsync` | Transition to `Failed` | Records `JobFailureInformation`. | Zero reflection |
| Retry | Persistence | `IJobStorageCapability.RetryAsync` | Transition to `Queued` | Increments `AttemptCount`, sets `NextExecutionTime`. | Zero reflection |
| Cancel | Persistence | `IJobStorageCapability.CancelAsync` | Transition to `Cancelled` | Terminates queue wait or execution. | Zero reflection |
| Tenant | Core | `TenantContext` | Existing properties | Wrapped in `JobExecutionContext`, serialized to DB. | Re-established in worker. |
| Identity | Core | `IdentityContext` | Existing properties | Wrapped in `JobExecutionContext`, serialized to DB. Credentials explicitly excluded. | Re-established in worker. |
| Authorization | Core | `AuthorizationContext` | Existing properties | Wrapped in `JobExecutionContext`, serialized to DB. | Re-established in worker. |

These contracts explicitly decouple the execution payload from the underlying queueing mechanism. No PostgreSQL-specific or infrastructure types may leak into these canonical contracts.

## Core Amendment
A controlled additive amendment to `SaaSFoundry.EngineeringWorkbench.Core` is REQUIRED to introduce these primitives without modifying existing functionality. The freeze version progresses to Architecture Freeze v1.0.3.

## DECISION
CONTRACT_REQUIRED
CANONICAL OWNER: SaaSFoundry.EngineeringWorkbench.Core
MINIMUM CONTRACT SET: IBackgroundJob, IBackgroundJobHandler<TJob>, JobExecutionContext, EnqueuedJob, JobStatus, JobFailureInformation, IJobPayloadSerializer, IJobStorageCapability
CORE AMENDMENT: REQUIRED (v1.0.3)
