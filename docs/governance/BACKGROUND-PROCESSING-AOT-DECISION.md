# BACKGROUND PROCESSING AOT DECISION

## Decision ID
DEC-2026-08-01-004

## Problem
The previously approved mechanism for Background Processing (Hangfire) introduces a conflict with the platform's NativeAOT invariant (`IsAotCompatible=true`). Hangfire relies extensively on dynamic reflection, runtime type discovery, and dynamic method activation to serialize and invoke background jobs. We must evaluate whether an architectural static-dispatcher wrapper can successfully shield the application from these AOT incompatibilities, or if Hangfire is fundamentally incompatible with the platform constraints.

## Static Dispatch Architecture
The proposed mitigation is a Static Job Dispatcher. Instead of Hangfire invoking the application logic directly (e.g., `BackgroundJob.Enqueue(() => DoWork())`), Hangfire would only ever invoke a single, statically known `StaticJobDispatcher.Dispatch(string jobId, string payloadJson)`. The dispatcher would use a compile-time registry to manually route the call to the appropriate strongly-typed handler.

## Compatibility Analysis
While the Static Dispatcher removes reflection from the *application's* job handlers, it does **not** remove reflection from Hangfire itself.
1. **Serialization**: Hangfire's core libraries serialize `MethodInfo` and expressions for the dispatch action. 
2. **Server Execution**: Hangfire's internal worker mechanism relies on `Activator.CreateInstance` and `Type.GetType` to reconstruct the `StaticJobDispatcher` type during job processing.
3. **NuGet References**: Simply adding the Hangfire libraries to a NativeAOT-configured project triggers IL2026 / IL3050 warnings from the .NET compiler because the Hangfire assembly attributes and internal logic are fundamentally hostile to static analysis and trimming.

Because the governance rule explicitly forbids suppressing genuine AOT warnings (NoWarn/IL2026 suppression) or hiding trimming warnings to claim false compatibility, the static dispatcher wrapper is insufficient. The AOT incompatibility is inherited from the framework core itself.

## Options Evaluated
- **OPTION A (Isolated Adapter)**: Rejected. Hangfire's internal dependency on `Type.GetType` and dynamic `Activator` cannot be fully isolated behind an adapter because the Hangfire Server itself must perform dynamic activation of the entry point.
- **OPTION B (Accept Limitations)**: Rejected. The platform enforces strict NativeAOT compatibility. Accepting undocumented reflection compromises the architectural invariants and causes guaranteed build failures under the current `EnableAotAnalyzer=true` rule.
- **OPTION C (Change Mechanism)**: Required. A different background processing mechanism that does not fundamentally depend on reflection for task dispatch (or supports source-generated dispatch) must be selected.

## DECISION
AOT_INCOMPATIBLE
