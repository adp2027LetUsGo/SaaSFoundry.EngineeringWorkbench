# ARCHITECTURE FREEZE AMENDMENT: gRPC INTER-CELL COMMUNICATION v1.0.6

## 1. Internal Cell Transport
gRPC is the sole canonical internal transport mechanism for communication between Generated Product Runtime Cells (`Core.Cell`, `Ingestor.Cell`, `Bridge.Cell`, `System.Cell`).

## 2. External Shopify Integration
Shopify GraphQL remains an external integration owned exclusively by `Bridge.Cell`. gRPC is strictly internal and must not be exposed to Shopify.

## 3. Synchronous Semantics
gRPC request/response is used for synchronous communication requiring immediate results (e.g., cross-Cell data lookups, immediate validation). 

## 4. Asynchronous Semantics (System.Cell)
All asynchronous durable work must be enqueued via synchronous gRPC calls to `System.Cell`. `System.Cell` utilizes `IJobStorageCapability` to persist the job in its durable PostgreSQL job queue.

## 5. TenantContext Propagation
`TenantContext` must be propagated across Cell boundaries using standard gRPC Metadata headers (e.g., `x-tenant-id`). The destination Cell must strictly enforce this context for all database operations.

## 6. IdentityContext Propagation
`IdentityContext` must be propagated via gRPC Metadata (e.g., `x-identity-id`, `x-identity-role`). Raw credentials, passwords, and external tokens (like Shopify tokens) must NOT be transmitted over this internal boundary.

## 7. AuthorizationContext Trust Model
Authorization is **propagated but independently re-evaluated** by the destination Cell. The destination Cell applies its own least-privilege Zero Trust checks against the propagated Identity and Tenant contexts.

## 8. Cell-to-Cell Trust Model

**CANONICAL CELL TRUST: mTLS**

SECURITY LAYERS:
1. mTLS authenticates the calling Cell.
2. `AuthenticationContext`/`IdentityContext` represent application identity.
3. `AuthorizationContext` is independently evaluated by the destination Cell.
4. `TenantContext` establishes the tenant boundary.
5. PostgreSQL RLS enforces local database isolation.

Cells must establish trust via mTLS, proving that the caller is an authenticated internal Cell, distinct from end-user or Shopify authentication.

**EXPLICITLY REJECTED**:
- Internal signed JWT is NOT the canonical Cell trust mechanism.
- Platform JWT is NOT the canonical Cell trust mechanism.
- API Keys are NOT the canonical Cell trust mechanism.
- Shopify credentials are NOT the canonical Cell trust mechanism.

## 9. Trace Propagation
Distributed tracing (TraceId, SpanId) propagates automatically via standard W3C Trace Context inside gRPC Metadata, ensuring End-to-End observability across Cell boundaries.

## 10. Timeout and Failure Semantics
- **Timeout**: gRPC Deadlines must be explicitly configured for all calls.
- **Failures**: Transient failures must utilize circuit breakers and bounded retries. Permanent failures map to standard gRPC Status Codes.
- **Asynchronous**: Reuse `IBackgroundJobHandler` and `IJobStorageCapability` for durable retries.

## 11. Idempotency Policy
Cross-Cell commands that mutate state must include an Idempotency Key in the gRPC Metadata. Destination Cells are responsible for enforcing idempotency.

## 12. API Versioning
Inter-Cell APIs must be versioned at the protobuf package level (e.g., `vibestock.core.v1`). Breaking changes require a new version namespace to prevent incompatible deployments.

## 13. Allowed Communication Directions
- `Core.Cell` ↔ `Ingestor.Cell` (Allowed)
- `Core.Cell` ↔ `Bridge.Cell` (Allowed)
- `Core.Cell` → `System.Cell` (Allowed, for job submission)
- `Ingestor.Cell` → `System.Cell` (Allowed, for job submission)
- `Bridge.Cell` → `System.Cell` (Allowed, for job submission)
- `Bridge.Cell` ↔ `Core.Cell` (Allowed, for catalog mapping)

## 14. Database Boundary
Database-per-Cell is absolute. A Cell must never access another Cell's PostgreSQL database directly. All cross-domain data access must utilize the gRPC boundary.

## 15. Native AOT
The entire gRPC stack must be strictly NativeAOT compatible. Reflection and runtime emit are forbidden. The stack must utilize `grpc-dotnet` with zero-reflection trimming.

## 16. Protobuf / Source-Generation
Serialization must be performed via AOT-compatible protobuf source generators. Dynamic proto parsing is prohibited.

## 17. Core Contract Ownership
**Architecture Freeze v1.0.6** is hereby established. Canonical transport and context propagation contracts will be introduced to `SaaSFoundry.EngineeringWorkbench.Core`.

## 18. Implementation Prerequisites
Implementation may only proceed once this amendment is formally approved, followed by the introduction of the Core contracts.
