# ARCHITECTURE FREEZE AMENDMENT: INTER-CELL CONTRACT SIGNATURES v1.0.7

## 1. Purpose
Resolves the final blockers in Architecture Freeze v1.0.6 by defining the exact, canonical C# contract signatures required in `SaaSFoundry.EngineeringWorkbench.Core` for inter-Cell communication.

## 2. Relationship to v1.0.6
This amendment extends v1.0.6 by providing exact C# representations for the policies defined previously. No new policies are introduced.

*(Note: Canonical Cell certificate identity representation is formally defined in the [Architecture Freeze v1.0.8 Amendment](ARCHITECTURE-FREEZE-AMENDMENT-GRPC-INTERCELL.v1.0.8.md))*

## 3. Approved gRPC Architecture
gRPC is the internal transport. 

## 4. Approved mTLS Trust Model
mTLS is the canonical Cell trust mechanism.

## 5. Context Propagation Model
Context propagates via gRPC metadata headers. The exact keys are defined in a canonical constant class to ensure deterministic coupling across independent Cells.

## 6. Tenant Propagation
`TenantContext` propagates as a simple string via the `x-tenant-id` metadata key. The destination Cell reconstructs `TenantContext` safely from this key, validating its own isolation boundary.

## 7. Identity Propagation
`IdentityContext` propagates via discrete metadata keys (Subject, Type, TenantAssociation). Claims are NOT blindly propagated to avoid token bloating and credential leakage. 

## 8. Authorization Propagation
**NO CORE AUTHORIZATION TRANSPORT CONTRACT REQUIRED.**
Because the destination Cell independently re-evaluates authorization using the provided Identity and Tenant, the `AuthorizationContext` is not transmitted.

## 9. Cell Identity
**NO CORE CELL IDENTITY CONTRACT REQUIRED.**
The four canonical Cell identities (`Core.Cell`, `Ingestor.Cell`, `Bridge.Cell`, `System.Cell`) are configuration strings validated at the mTLS infrastructure layer (e.g. via SAN validation).

## 10. Idempotency
An Idempotency Key propagates via the `x-idempotency-key` metadata key. The canonical enforcement abstraction is added to Core.

## 11. Trace Propagation
**NO CORE TRACE CONTRACT REQUIRED.**
W3C Trace Context (`traceparent`, `tracestate`) is standard. Propagation is handled entirely by OpenTelemetry/.NET runtime infrastructure.

## 12. Deadline Policy
**NO CORE DEADLINE CONTRACT REQUIRED.**
gRPC deadlines (`grpc-timeout`) are a runtime infrastructure concern handled by `grpc-dotnet` clients.

## 13. Retry Policy
**NO CORE RETRY CONTRACT REQUIRED.**
Synchronous retries belong to Polly policies in the runtime. Asynchronous retries already belong to the existing `IJobStorageCapability`.

## 14. Versioning
**NO CORE VERSIONING CONTRACT REQUIRED.**
Protobuf package versioning (`vibestock.core.v1`) is an infrastructure-level proto definition concern.

## 15. Core Ownership Analysis
Only the metadata constants and the idempotency enforcement abstraction genuinely belong in Core. All other concerns (mTLS, tracing, retries, protobuf) belong exclusively to the Generated Product Runtime.

## 16. Exact C# Contracts

These are the ONLY contracts authorized for addition to `SaaSFoundry.EngineeringWorkbench.Core`:

```csharp
namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Transport
{
    public static class InterCellMetadataKeys
    {
        public const string TenantId = "x-tenant-id";
        public const string IdentitySubjectId = "x-identity-subject-id";
        public const string IdentityType = "x-identity-type";
        public const string IdentityTenantAssociation = "x-identity-tenant-association";
        public const string IdempotencyKey = "x-idempotency-key";
    }

    public interface IIdempotencyEnforcer
    {
        System.Threading.Tasks.Task<bool> IsAlreadyProcessedAsync(string idempotencyKey, System.Threading.CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task RecordAsProcessedAsync(string idempotencyKey, System.Threading.CancellationToken cancellationToken = default);
    }
}
```

## 17. Runtime-Only Responsibilities
- mTLS certificate validation (`ServerCertificateCustomValidationCallback`)
- gRPC client factory configuration
- Protobuf source generation
- W3C Trace Context injection/extraction
- Deadlines and Polly circuit breakers

## 18. AOT Requirements
Core contracts must remain 100% NativeAOT compatible (no reflection).

## 19. Reflection Prohibition
Strictly enforced.

## 20. Dependency Constraints
The new `SaaSFoundry.EngineeringWorkbench.Core.Contracts.Transport` namespace MUST NOT reference `Grpc.Core`, `Grpc.Net.Client`, `Google.Protobuf`, or `System.Security.Cryptography.X509Certificates`.

## 21. Implementation Prerequisites
Update Core with these exact signatures and add regression tests.

## 22. Certification Requirements
The `SaaSFoundry.EngineeringWorkbench.Core.UnitTests` must pass and verify exact constant values and interface structures.
