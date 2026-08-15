# ARCHITECTURE FREEZE AMENDMENT PROPOSAL: BACKGROUND JOBS

## Amendment ID
AMEND-2026-08-01-002

## Status
PROPOSED

## Target
`SaaSFoundry.EngineeringWorkbench.Core`

## Target Freeze Version
Architecture Freeze v1.0.1 -> v1.0.2

## Problem
The canonical `SaaSFoundry.EngineeringWorkbench.Core` project does not currently define canonical job contracts to represent a background job payload or its execution context. Because Platform runtime contracts must reside in the Golden Reference (Core), the `SaaSFoundry.Plugins.BackgroundProcessing` plugin cannot safely proceed without inventing proprietary, non-canonical primitives.

## Proposal
Introduce the following deterministic, AOT-compatible primitive contracts to `SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs`:

1. `IBackgroundJob`: A marker interface for strongly-typed job payloads.
2. `JobExecutionContext`: A deterministic context record wrapping the execution environment.
3. `IBackgroundJobHandler<TJob>`: A handler interface parameterized over `TJob` where `TJob : IBackgroundJob`.

```csharp
namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;

using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;

public interface IBackgroundJob
{
    string JobTypeId { get; }
}

public record JobExecutionContext(
    IdentityContext Identity,
    TenantContext Tenant,
    AuthorizationContext Authorization
);

public interface IBackgroundJobHandler<in TJob> where TJob : IBackgroundJob
{
    Task ExecuteAsync(TJob job, JobExecutionContext context, CancellationToken cancellationToken);
}
```

## Impact Analysis
- **Core Stability**: Additive only. No existing types are modified. 
- **SDK Stability**: Zero impact. No backwards references created.
- **NativeAOT**: Fully compatible. No reflection needed for definitions.
- **Dependencies**: Uses existing `IdentityContext`, `TenantContext`, and `AuthorizationContext` introduced in v1.0.1.

## Conclusion
This amendment satisfies the Platform requirement that core execution mechanics belong in the canonical repository layer, preventing plugin-local duplication.
