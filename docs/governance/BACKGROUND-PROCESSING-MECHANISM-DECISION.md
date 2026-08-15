# BACKGROUND PROCESSING MECHANISM DECISION

## Decision ID
DEC-2026-08-01-005

## Problem
The previously selected background processing mechanism (Hangfire) has been explicitly rejected because its core execution model requires dynamic method activation and extensive reflection, which violates the strict NativeAOT architectural invariant. A replacement mechanism must be selected that fulfills VibeStock's requirements for durable, resilient background jobs while strictly adhering to AOT compatibility and zero reflection.

## NativeAOT Constraint
The platform mandates `<IsAotCompatible>true</IsAotCompatible>`. Any replacement mechanism must be structurally capable of running without `Type.GetType`, `Activator.CreateInstance`, or `MethodInfo.Invoke`.

## Candidates Evaluated

### Option A: In-Process Channels + BackgroundService
A simple in-memory queue using `System.Threading.Channels` and an `IHostedService` worker.
- **Pros**: 100% NativeAOT compatible. Extremely low latency.
- **Cons**: No persistence. Jobs are lost on crash. Cannot scale horizontally across multiple workers. Lacks delayed and recurring features without complex custom code. 

### Option B: Dedicated Queue Infrastructure
Using an external message broker (RabbitMQ, Azure Service Bus) with AOT-compatible clients and a static handler registry.
- **Pros**: Highly scalable, durable, good multi-worker support.
- **Cons**: Very high operational complexity. Introduces new infrastructure components that contradict the streamlined operational model desired for VibeStock.

### Option C: PostgreSQL-Backed Queue + Static Dispatcher + BackgroundService
A custom queue implementation using the existing PostgreSQL database. It utilizes `SELECT ... FOR UPDATE SKIP LOCKED` for concurrent multi-worker dequeuing, a `BackgroundService` for the polling/execution loop, and a static compile-time registry mapping string IDs to typed `IBackgroundJobHandler<T>` instances.
- **Pros**: 100% NativeAOT compatible. Persistent, durable, supports retries, delayed, and recurring jobs via database state. Highly scalable across multiple workers. Operational complexity is low because it reuses existing PostgreSQL infrastructure.

## Comparison Matrix
(1 = poor, 3 = acceptable, 5 = strong)

| Criterion | Option A (Channels) | Option B (Broker) | Option C (PostgreSQL) |
| :--- | :--- | :--- | :--- |
| NativeAOT | 5 | 5 | 5 |
| Persistence | 1 | 5 | 5 |
| Retries | 3 | 5 | 5 |
| Delayed Jobs | 1 | 3 | 5 |
| Recurring Jobs | 1 | 3 | 5 |
| Crash Recovery | 1 | 5 | 5 |
| Multi-worker | 1 | 5 | 5 |
| Tenant Isolation | 5 | 3 | 5 |
| Identity Propagation | 5 | 3 | 5 |
| Observability | 3 | 5 | 5 |
| Operational Complexity | 5 | 1 | 5 |
| VibeStock Suitability | 1 | 3 | 5 |

## Selected Architecture: Option C (PostgreSQL-Backed Queue)
Option C perfectly balances the requirement for robust, durable background processing (persistent jobs, retries, multi-worker) with NativeAOT compatibility and low operational overhead (by reusing the canonical Persistence plugin).

## Rejected Alternatives
- **Hangfire**: Rejected due to fundamental AOT incompatibility.
- **Option A (Channels)**: Rejected due to lack of durability and crash recovery.
- **Option B (Broker)**: Rejected due to excessive operational complexity.

## Job Contract Relationship
The canonical `IBackgroundJob`, `IBackgroundJobHandler`, and `JobExecutionContext` reside in `SaaSFoundry.EngineeringWorkbench.Core`. The PostgreSQL queue implementation acts solely as the transport and scheduling layer, completely decoupled from the payload definition.

## Tenant Model
The queue table will include a `TenantId` column. The enqueueing capability serializes the payload. The worker loop deserializes the payload, re-establishes the exact `TenantContext`, and executes the job. All subsequent Persistence calls within the handler correctly apply PostgreSQL RLS based on that `TenantContext`.

## Identity Model
The queue table and serialized job metadata will capture the `IdentityContext` representing the system/background actor. No raw secrets, JWTs, or API keys will be persisted.

## Persistence Model
BackgroundProcessing relies on `SaaSFoundry.Plugins.Persistence` for all database interactions. Background processing tables will be migrated and managed via the established persistence patterns.

## Scheduling
Delayed jobs will be supported via a `NextExecutionTime` column in the queue table. Recurring jobs will be managed via a separate configuration table that enqueues discrete job instances on a timer.

## Retry
Retries will be supported natively via `AttemptCount`, `MaxAttempts`, and `FailureState` columns in the job table, calculating exponential backoff for `NextExecutionTime`.

## Worker Model
`Microsoft.Extensions.Hosting.BackgroundService` will run polling loops concurrently. Locking is handled via PostgreSQL `FOR UPDATE SKIP LOCKED`.

## Observability
The BackgroundService loop explicitly wraps job execution in telemetry scopes, emitting Queued, Started, Completed, Failed, Retried, and Cancelled events to the canonical Observability pipeline.

## API Boundary
`API -> BackgroundProcessing`. API endpoints may inject enqueue capabilities, but BackgroundProcessing remains agnostic of the API layer.

## DECISION
APPROVED
