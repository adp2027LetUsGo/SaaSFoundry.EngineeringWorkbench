# Architecture Freeze Amendment v1.0.5 — Generated Product Runtime

## Objective
Establish the architectural boundary for the Generated Product Runtime without modifying the existing frozen platform contracts in `SaaSFoundry.EngineeringWorkbench.Core`.

## Rationale
The `IJobStorageCapability` contract (frozen in v1.0.4) requires a concrete implementation that executes against PostgreSQL using Npgsql and Dapper.AOT. The `SaaSFoundry.Plugins.Persistence` plugin has been determined to be strictly an authoring-time artifact generator. Modifying it to execute database queries would violate the SDK dependency boundary. A new boundary—the Generated Product Runtime—must be established to own the execution of generated artifacts and external dependencies.

## Boundary Definitions

### 1. Generated Product Runtime
- **Definition**: The independently compilable/executable target application (e.g., VibeStock).
- **Responsibilities**: Consumes generated source artifacts, references external NuGet packages (`Npgsql`, `Dapper.AOT`, etc.), and executes the concrete `IJobStorageCapability` implementation.
- **Dependencies**: May reference `SaaSFoundry.EngineeringWorkbench.Core` to consume canonical runtime contracts.

### 2. Workbench Plugins
- **Definition**: Authoring-time SDK components (e.g., `SaaSFoundry.Plugins.Persistence`).
- **Responsibilities**: Generates source artifacts, validates inputs, and integrates with the SDK.
- **Restrictions**: MUST NOT execute generated artifacts, and MUST NOT take direct runtime dependencies on external databases or execution frameworks (like Npgsql) for the purpose of executing the target application's workload.

## Core Contract Status
All contracts frozen in v1.0.4 (including `IJobStorageCapability`, `IJobContextSerializer`, `EnqueuedJob`, `JobExecutionContext`, `IdentityContext`, `TenantContext`, `AuthorizationContext`) remain **UNCHANGED** and fully active.

## Required Actions
Future implementation stages MUST direct all PostgreSQL/Dapper.AOT runtime code into the Generated Product Runtime (or its generated source artifacts), leaving the plugins strictly as generators.
