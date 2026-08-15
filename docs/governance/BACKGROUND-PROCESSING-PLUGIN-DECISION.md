# BACKGROUND PROCESSING PLUGIN DECISION

## 1. Plugin Role
`SaaSFoundry.Plugins.BackgroundProcessing` has been industrialized as an artifact generator. It is not a runtime execution library. It operates strictly during the factory phase to generate the infrastructure required to execute background jobs within a generic Product Cell.

## 2. Generator/Runtime Boundary
The plugin generates the BackgroundProcessing runtime components:
- `BackgroundWorkerService`: Polling loop that fetches and executes jobs.
- `StaticJobDispatcher`: Coordinates statically registered job execution.
- `JobPayloadSerializer`: Serializes typed jobs to JSON.
- `JobContextSerializer`: Serializes safe context state to JSON.

The product's actual jobs (`TestJob`, `SyncJob`, etc.) remain out of the plugin generator.

## 3. Capabilities
The plugin currently exposes:
- **BackgroundJobCapability**: Generates the necessary generic infrastructure and exposes extension points (`partial` classes and methods) to allow product developers to register jobs via compile-time static dispatch without dynamic registries.

## 4. Static Dispatch
Instead of relying on a dynamic runtime registry, `StaticJobDispatcher` is generated as a `partial` class with a `partial void TryDispatch` extension point. The product runtime (e.g., `VibeStock`) provides the concrete implementation of this method with a hardcoded `switch` statement for optimum execution speed and native AOT compatibility.

## 5. Serialization Boundary
- **Payload Serialization**: Uses `JobPayloadSerializer` generated as a `partial` class with `TrySerialize` and `TryDeserialize` extension points, enabling the product runtime to utilize `JsonSerializerContext` directly.
- **Context Serialization**: Explicitly generates `JobContextSerializer`.

## 6. Persistence Boundary
The generated components do not reference `Npgsql` or `Dapper.AOT`. They depend entirely on the canonical `IJobStorageCapability` contract. Concurrency limits and `FOR UPDATE SKIP LOCKED` remain correctly encapsulated inside the concrete persistence runtime implementation.

## 7. Authentication Context
The `JobContextSerializer` explicitly purges the `AuthenticationContext` upon serialization, ensuring it is never durably stored, while retaining `Identity` and `Tenant`.

## 8. AOT Strategy
NativeAOT compatibility is strictly enforced:
- No `System.Reflection`.
- No `dynamic` dispatch.
- No `Activator.CreateInstance`.
- No dynamic type resolution (`Type.GetType`).
- Extension points leverage C# partial class and partial method constructs to resolve code paths at compile time.

## 9. Certification Result
The BackgroundProcessing Plugin generator is fully **CERTIFIED**. The artifacts it generates exactly mirror the validated Stage 9G.4M prototype, but generalized.

## 10. Relationship to Certified Runtime
The generated `BackgroundWorkerService` is functionally identical to the one certified in Stage 9G.4M, with the namespace extracted and isolated from VibeStock business logic.

## 11. Relationship to Factory v1.0
With this plugin completed, the core infrastructure capabilities of Factory v1.0 (API, Auth, Observability, Persistence, Background Processing) are fully industrialized. Factory v1.0 is now **READY TO FREEZE**.
