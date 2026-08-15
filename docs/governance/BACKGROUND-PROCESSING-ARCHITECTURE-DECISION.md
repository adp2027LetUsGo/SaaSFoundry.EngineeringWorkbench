# BACKGROUND PROCESSING ARCHITECTURE DECISION

## Decision ID
DEC-2026-08-01-002

## Problem
VibeStock and the EngineeringWorkbench require a background processing mechanism for Shopify ingestion, bulk imports, exports, product/inventory sync, long-running transformations, retries, delayed execution, recurring sync, and failure recovery.
We need a canonical BackgroundProcessing plugin that maintains strict Tenant isolation, respects Identity context, avoids persisting secrets, supports NativeAOT (or explicitly documents constraints), and integrates with the existing Observability and Persistence plugins.

## Product Requirements
- Shopify ingestion
- Bulk imports & exports
- Product & Inventory synchronization
- Long-running transformations
- AI processing
- Retries & failure recovery
- Delayed and recurring execution
- Strict Tenant isolation (PostgreSQL RLS)
- Operational observability (no secret logging)

## Existing Evidence
`REFERENCE-PLUGIN-STANDARD.md` explicitly lists "Hangfire Plugin" as a BAD plugin name, contrasting it with "Background Processing" as a GOOD name. This indicates Hangfire was the intended underlying mechanism for the background processing domain in the repository canon.

## Candidate Comparison
Scores: 1 = poor, 3 = acceptable, 5 = strong

| Feature | Hangfire | IHostedService | Dedicated Queue |
| :--- | :--- | :--- | :--- |
| .NET 10 Compatibility | 5 | 5 | 5 |
| NativeAOT Compatibility | 1 (Reflection heavy)* | 5 | 5 |
| Deterministic Execution Model | 3 | 5 | 5 |
| Persistent Jobs | 5 | 1 (Requires custom) | 5 |
| Retries | 5 | 1 (Requires custom) | 5 |
| Delayed Jobs | 5 | 1 (Requires custom) | 5 |
| Recurring Jobs | 5 | 1 (Requires custom) | 5 |
| Failure Recovery | 5 | 1 | 5 |
| Tenant Propagation | 3 (Needs custom filter) | 3 | 3 |
| Observability | 5 | 3 | 5 |
| Operational Simplicity | 5 (Dashboard built-in) | 3 | 1 (More infra) |
| VibeStock Suitability | 5 | 1 | 3 |

*Hangfire fundamentally relies on `System.Reflection`, `MethodInfo`, and `Activator.CreateInstance` for job invocation, which contradicts the platform's NativeAOT and zero-reflection constraints.

## Selected Mechanism
Hangfire

## Rejected Alternatives
- **IHostedService / BackgroundService**: Lacks out-of-the-box persistent state, automatic retries, recurring scheduling, failure recovery, and an operational dashboard. Building these from scratch violates "operational simplicity" and risks introducing unstable custom infrastructure.
- **Dedicated Queue/Worker (e.g., RabbitMQ, MassTransit)**: Introduces heavy external infrastructure dependencies unnecessarily complex for VibeStock's current scale.

## Job Model
Jobs will be represented by canonical platform contracts (e.g., a serialized Job payload or command). The job definition is separated from the runtime execution instance.

## Tenant Model
The `TenantContext` must be explicitly serialized alongside the job arguments (excluding any untrusted HTTP TenantIds) so that it can be restored during execution. When the background job executes, the `TenantContext` is re-established in the worker thread to ensure Persistence/PostgreSQL RLS applies correctly.

## Identity Model
The `IdentityContext` (representing Machine-to-Machine or system background identities) must be explicitly propagated. User credentials, JWTs, and API Keys MUST NOT be persisted. The job executes under a trusted background execution identity mapped to the tenant. If current Core contracts don't define a specific `BackgroundJobIdentity`, a standard `IdentityContext` representing the system/tenant actor will be used.

## Retry Model
Uses Hangfire's built-in automatic retry mechanics, explicitly integrated with the canonical Observability pipeline to log retry attempts.

## Scheduling Model
Uses Hangfire's Delayed and Recurring job features.

## Persistence Boundary
BackgroundProcessing depends on Persistence for its job storage (e.g., using PostgreSQL). It will use `SaaSFoundry.Plugins.Persistence` for connection management or configure Hangfire's storage using canonical connection definitions.

## API Boundary
API can enqueue jobs by calling the `BackgroundProcessing` capabilities. BackgroundProcessing does NOT depend on API. Flow is strictly `API -> BackgroundProcessing`.

## Observability Boundary
Custom Hangfire Job Filters will integrate with the canonical Observability capabilities to record events: Queued, Started, Completed, Failed, Retried, Cancelled. Raw payloads, API keys, and secrets will be explicitly masked/omitted.

## AOT Implications
**CRITICAL LIMITATION**: Hangfire relies heavily on `System.Reflection`, `Type.GetType`, and `Activator.CreateInstance` to deserialize and invoke job methods. 
Since the platform requires strictly NativeAOT compliant code (`IsAotCompatible=true`), Hangfire's default behavior will produce AOT warnings and runtime errors.
**Resolution**: Record this as an architectural constraint. Hangfire's NativeAOT limitations mean the plugin will either need to abstract job execution behind a deterministic scheme (e.g., a static Job Dispatcher / Registry pattern mapping string IDs to action delegates) to avoid dynamic method invocation, or explicitly document the reflection warnings as acceptable for this specific plugin.

## Implementation Requirements
Implement `SaaSFoundry.Plugins.BackgroundProcessing` integrating Hangfire for persistent background jobs while fulfilling tenant isolation, observability, and authentication policies.
