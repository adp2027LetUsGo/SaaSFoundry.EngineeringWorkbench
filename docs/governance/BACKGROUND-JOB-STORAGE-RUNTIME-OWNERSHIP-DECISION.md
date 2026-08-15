# Background Job Storage Runtime Ownership Decision

## Status
ARCHITECTURAL GAP IDENTIFIED

## Problem
The `IJobStorageCapability` contract has been added to `SaaSFoundry.EngineeringWorkbench.Core` as part of Architecture Freeze v1.0.4. A concrete implementation of this contract is required to store durable background jobs.

The STAGE 9G execution plan initially mandated that the implementation be placed inside `SaaSFoundry.Plugins.Persistence`. However, an architectural audit has confirmed that this is a generator plugin, not a runtime database execution layer.

## Evidence
- `SaaSFoundry.Plugins.Persistence` contains capabilities (`ConnectionCapability`, `QueryCapability`) that generate C# source text containing `Npgsql` and `Dapper.AOT` configurations (e.g., `NpgsqlConnectionSetup.cs`, `DapperQuerySetup.cs`).
- There are no `Npgsql` or `Dapper.AOT` NuGet package references in the entire `SaaSFoundry.EngineeringWorkbench.sln`.
- There is no host or runtime application project in the repository that acts as the consumer of the generated database adapters. The Workbench acts strictly as an SDK/Agent layer that generates artifacts.
- Modifying `SaaSFoundry.Plugins.Persistence` to act as a runtime execution library by adding NuGet dependencies and executable SQL queries would violate the SDK dependency boundary and its established pattern as an `ITraceablePluginCapability` code generator.

## Missing Boundary
Because the target application project (the runtime consumer) does not exist within the workbench repository itself, there is no canonical location to place the compiled C# implementation of `IJobStorageCapability`.

The current architecture is:
`SaaSFoundry.Plugins.Persistence` -> [Generates] -> `PostgreSQL / Dapper.AOT Source Artifacts` -> [Compiled into] -> **MISSING TARGET RUNTIME APPLICATION**

## Dependency Implications
- `SaaSFoundry.Plugins.BackgroundProcessing` expects to dispatch jobs against `IJobStorageCapability`.
- The actual durable store requires PostgreSQL and `Npgsql`/`Dapper.AOT`.
- The implementation must either be:
  1. A new generated source artifact produced by `SaaSFoundry.Plugins.Persistence` (e.g., `JobStorageCapability.cs` which generates `PostgreSqlJobStorage.cs`).
  2. Placed in a brand new runtime infrastructure project within the repository if the Workbench itself intends to use background jobs.

## Minimum Required Architecture Decision
A canonical decision must be made regarding whether `IJobStorageCapability` is meant to be consumed by the Workbench itself (requiring a new runtime infrastructure project) or by the generated target applications (requiring `SaaSFoundry.Plugins.Persistence` to generate the implementation as C# artifacts).
