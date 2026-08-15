# ARCHITECTURE FREEZE AMENDMENT: RLS BOUNDARY & HOSTING MODEL v1.1.0

## 1. Problem Statement
Stage 9N.4C discovered two architectural gaps:
1. **RLS Persistence Boundary**: `IJobStorageCapability` implementations (e.g., `SystemCellJobStorage`) own the `NpgsqlConnection`, making it impossible for the gRPC transport layer to inject PostgreSQL RLS context before job enqueueing.
2. **Generated Runtime Hosting**: Factory v1.0 generates Cells as `Microsoft.NET.Sdk` console applications. Hosting `grpc-dotnet` requires ASP.NET Core, raising concerns about SDK changes and regeneration loss.

## 2. RLS Boundary and Job Queue Semantics
Investigation confirms that the `System.Cell` durable job queue (`SystemCellJobs`) is a **cross-tenant, system-level table**. 
- **Queue Operations**: `EnqueueAsync` and `ClaimNextAsync` are SYSTEM-LEVEL operations. They do not require, and must not enforce, tenant-scoped RLS.
- **Tenant Execution**: The tenant isolation boundary applies *after* a job is claimed. The background worker deserializes the opaque `SerializedContext`, reconstructs the `TenantContext`, and establishes RLS on the execution connection prior to invoking the domain job handler.
- **Connection Ownership**: The persistence runtime owns the connection and transaction.
- **Core Impact**: Zero. `IJobStorageCapability` remains unchanged. The opaque `SerializedContext` boundary is preserved.

## 3. Generated Runtime Hosting Model
Investigation confirms that a `Microsoft.NET.Sdk` project can natively host a NativeAOT-compatible `grpc-dotnet` server by adding an explicit `<FrameworkReference Include="Microsoft.AspNetCore.App" />`. It does not require changing the project SDK to `Microsoft.NET.Sdk.Web`.

## 4. Factory v1.0 Compatibility Gap
While the hosting model is technically sound, manually adding the `FrameworkReference` to the generated `.csproj` files violates the regeneration invariant, as Factory v1.0 would overwrite these modifications. A Factory amendment is formally required to enable persistent injection of ASP.NET Core framework references into Generated Product Runtime projects.

## 5. Native AOT & Reflection
The architecture preserves the NativeAOT mandate. No reflection, dynamic code generation, or runtime assembly scanning is permitted. 

## 6. Required Actions
A **FACTORY AMENDMENT REQUIRED** status is declared to update the frozen Factory v1.0 to support injecting the required ASP.NET Core framework reference without breaking the generation lifecycle.
