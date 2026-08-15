# ARCHITECTURE FREEZE AMENDMENT PROPOSAL: BACKGROUND JOBS (v1.0.4)

## Amendment ID
AMEND-2026-08-01-004

## Status
APPROVED FOR IMPLEMENTATION

## Target
`SaaSFoundry.EngineeringWorkbench.Core`

## Target Freeze Version
Architecture Freeze v1.0.3 -> v1.0.4

## Problem
The v1.0.3 amendment introduced `IJobStorageCapability` and `EnqueuedJob` passing `JobExecutionContext` directly into the Persistence boundary. However, `JobExecutionContext` contains complex domain models (`IdentityContext`, `TenantContext`, `AuthorizationContext`) with generic collections (`IReadOnlyDictionary`, `IReadOnlyList`). 

Passing the raw context into Persistence forces the Persistence implementation to either:
1. Dynamically serialize the objects using reflection-based JSON (`Npgsql` dynamic JSON), violating the strict NativeAOT requirement.
2. Introduce a custom `JsonSerializerContext` inside Persistence, inappropriately coupling the storage layer to application security domain models.
3. Invent flat substitute DTOs (e.g., `StoredTenantContext`), violating the "No substitute contracts" rule.

To resolve this contradiction and preserve the AOT invariants, an opaque serialization boundary for the context must be established, mirroring the existing payload serialization pattern.

## Canonical Context Serialization Decision

The canonical identity and security contracts must cross the durable boundary as an opaque string. 

- **IdentityContext**: SERIALIZADO
- **TenantContext**: SERIALIZADO
- **AuthorizationContext**: SERIALIZADO
- **AuthenticationContext**: NO SERIALIZADO
- **JobExecutionContext**: Serializado antes de cruzar el Persistence boundary.

The context must cross into `IJobStorageCapability` as a `string SerializedContext`. Persistence must treat `SerializedContext` exactly like `SerializedPayload`: as an opaque data string.

## Security Rule
`AuthenticationContext` MUST NOT form part of the durable context. JWTs, Bearer tokens, API Keys, passwords, secrets, private keys, and session credential materials must never be persisted in the job queue. 
The durable serialization mechanism must only process `IdentityContext`, `TenantContext`, and `AuthorizationContext`. No substitute DTOs (`StoredIdentityContext`, etc.) are permitted. The deserialization process must reconstruct a valid `JobExecutionContext` without the `AuthenticationContext`.

## Job Payload Separation
Job Payload serialization and Execution Context serialization must remain strictly isolated as two independent boundaries:

**Job Payload Boundary:**
```text
IBackgroundJob
      |
      | IJobPayloadSerializer
      v
string SerializedPayload
      |
      v
Persistence
```

**Execution Context Boundary:**
```text
JobExecutionContext
      |
      | IJobContextSerializer
      v
string SerializedContext
      |
      v
Persistence
```

## IJobContextSerializer Canonical Contract

```csharp
public interface IJobContextSerializer
{
    string Serialize(JobExecutionContext context);

    JobExecutionContext Deserialize(string serializedContext);
}
```

- Core owns the abstraction.
- BackgroundProcessing owns the concrete implementation.
- Persistence treats SerializedContext as opaque.
- System.Text.Json is NOT referenced by Core.
- JsonSerializerContext is NOT referenced by Core.
- AuthenticationContext is excluded.
- No reflection is permitted.
- No substitute DTOs are permitted.

## Context Serializer Responsibility
The serialization owner is **`SaaSFoundry.Plugins.BackgroundProcessing`**. 

BackgroundProcessing is responsible for:
- Defining the `System.Text.Json.Serialization.JsonSerializerContext` (source-generated metadata).
- Serializing and deserializing `JobExecutionContext`.
- Performing the static compile-time type registration.

`EngineeringWorkbench.Core` must remain free of `System.Text.Json` dependencies.
`SaaSFoundry.Plugins.Persistence` must remain free of JSON logic, dynamic JSON mapping, and complex domain mapping.

## EnqueuedJob v1.0.4
The durable envelope must be amended to remove the direct `Context` reference and store the serialized string.

**Current:**
```csharp
JobExecutionContext Context
```

**Target:**
```csharp
string SerializedContext
```

The final `EnqueuedJob` contains:
`JobId`, `JobTypeId`, `SerializedPayload`, `SerializedContext`, `Status`, `AttemptCount`, `NextExecutionTime`, `CreatedAt`, `StartedAt`, `CompletedAt`, `FailureInformation`.

## IJobStorageCapability v1.0.4
The persistence capability boundary must be amended to accept the serialized string.

**Current:**
```csharp
Task<string> EnqueueAsync(string jobTypeId, string serializedPayload, JobExecutionContext context, DateTimeOffset? nextExecutionTime, CancellationToken cancellationToken);
```

**Target:**
```csharp
Task<string> EnqueueAsync(string jobTypeId, string serializedPayload, string serializedContext, DateTimeOffset? nextExecutionTime, CancellationToken cancellationToken);
```

## Decision Matrix

| Concept | Owner | Canonical Contract | Serialized Representation | AOT Mechanism | Persistence Visibility |
| :--- | :--- | :--- | :--- | :--- | :--- |
| IdentityContext | Core | `IdentityContext` | Nested inside `SerializedContext` | `JsonSerializerContext` | None (Opaque) |
| TenantContext | Core | `TenantContext` | Nested inside `SerializedContext` | `JsonSerializerContext` | None (Opaque) |
| AuthenticationContext | Core | `AuthenticationContext` | **EXCLUDED** | N/A | None (Opaque) |
| AuthorizationContext | Core | `AuthorizationContext` | Nested inside `SerializedContext` | `JsonSerializerContext` | None (Opaque) |
| JobExecutionContext | Core | `JobExecutionContext` | `string SerializedContext` | `JsonSerializerContext` | None (Opaque) |
| SerializedContext | BackgroundProcessing | N/A (string) | `string` | Built-in | Full (Opaque string) |
| SerializedPayload | BackgroundProcessing | N/A (string) | `string` | Built-in | Full (Opaque string) |
| IJobPayloadSerializer | Core | `IJobPayloadSerializer` | N/A | AOT-safe Registry | None |
| IJobContextSerializer | Core | `IJobContextSerializer` | N/A | AOT-safe JSON Gen | None |
| EnqueuedJob | Core | `EnqueuedJob` | N/A | Plain Record | Full |
| IJobStorageCapability | Core | `IJobStorageCapability` | N/A | Interface | Implements |

## Architecture Diagram

```text
IdentityContext
TenantContext
AuthorizationContext
      |
      v
JobExecutionContext
      |
      | BackgroundProcessing
      | JsonSerializerContext
      v
string SerializedContext
      |
      v
IJobStorageCapability
      |
      v
Persistence
      |
      v
PostgreSQL
```

## AOT Requirement
The future implementation MUST use `System.Text.Json` and `JsonSerializerContext` with explicit source generation inside `BackgroundProcessing`.
It strictly forbids:
- `System.Reflection`
- `Type.GetType`
- `Activator.CreateInstance`
- `dynamic`
- assembly scanning
- `builder.EnableDynamicJson()` in Npgsql

## SDK Invariant
The SDK (`SaaSFoundry.SDK.Core`) remains completely untouched with zero internal dependencies.

## Conclusion
This amendment closes the NativeAOT serialization gap for Background Jobs, explicitly shifting context serialization responsibility to the BackgroundProcessing layer while preserving the opaqueness and simplicity of the Persistence boundary.
