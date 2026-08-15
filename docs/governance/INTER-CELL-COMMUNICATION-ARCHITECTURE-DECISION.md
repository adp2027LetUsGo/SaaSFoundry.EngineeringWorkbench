# INTER-CELL COMMUNICATION ARCHITECTURE DECISION

## 1. Executive Decision
**DECISION: CANONICAL MECHANISM IDENTIFIED AND AMENDED**

- **CORE AMENDMENTS**: Architecture Freeze v1.0.6 → v1.0.7 contract amendment → v1.0.8 mTLS identity amendment → v1.0.9 Idempotency contract amendment → v1.1.0 RLS and Hosting amendment.
- **CORE-OWNED**: InterCellMetadataKeys, IIdempotencyEnforcer, IdempotencyAcquisitionStatus
- **GENERATED-PRODUCT-RUNTIME-OWNED**: mTLS, Protobuf, gRPC Deadlines, Tracing, Retries

- **INTER-CELL TRANSPORT**: gRPC
- **CELL TRUST**: mTLS
- **SHOPIFY TRANSPORT**: Shopify GraphQL
- **USER AUTHENTICATION**: Existing Authentication architecture
- **AUTHORIZATION**: Destination-side independent evaluation
- **TENANCY**: TenantContext + Database-per-Cell + PostgreSQL RLS
- **OBSERVABILITY**: W3C Trace Context

## 2. Repository Evidence
Analysis of `src\SaaSFoundry.EngineeringWorkbench.Core`, `docs\governance`, and the reference plugins confirms that while internal event buses (`ILifecycleEventBus`) exist for the Workbench, no canonical runtime networking mechanism (such as gRPC, HTTP clients, or message brokers) is defined for cross-cell interaction in VibeStock.

## 3. Current Cell Topology
The architecture consists of four macro-cells (`Core.Cell`, `Ingestor.Cell`, `Bridge.Cell`, `System.Cell`), each operating as an independent process/container with its own PostgreSQL database. Cross-cell database access is strictly forbidden.

## 4. Communication Requirements
- Must preserve independent Cell processes and databases.
- Must propagate `TenantContext` to ensure data isolation.
- Must propagate `IdentityContext` and distributed tracing correlation IDs.
- Must be strictly NativeAOT compatible (no reflection or runtime assembly scanning).

## 5. Communication Semantics
The architecture requires both:
- **Synchronous Communication**: For direct queries or immediate validation (e.g., `Core.Cell` verifying an entity).
- **Asynchronous Communication**: For long-running workflows (e.g., `Bridge.Cell` enqueuing a Shopify synchronization job).

## 6. Tenant Propagation
`TenantContext` must be propagated across Cell boundaries (e.g., via HTTP headers or gRPC metadata) so the receiving Cell can securely initialize its own tenant scope without trusting raw payload identifiers.

## 7. Identity Propagation
`IdentityContext` must be propagated securely. Raw credentials or platform JWTs should not be passed around unnecessarily; a system actor or signed transport identity should be evaluated.

## 8. Authorization Propagation
Authorization is destination-side independent evaluation. The destination Cell independently evaluates authorization.

## 9. Observability Propagation
Distributed tracing (Trace ID, Span ID) must propagate across the transport to link requests and background jobs across Cells via W3C Trace Context.

## 10. Failure Semantics
- **Synchronous**: Requires timeout, transient retry semantics (circuit breakers), and clear permanent failure responses.
- **Asynchronous**: Requires at-least-once durable delivery via `System.Cell`'s background job infrastructure.

## 11. Candidate Transport Analysis
- **HTTP/REST (Source-Generated System.Text.Json)**: Highly compatible with NativeAOT. Standard header propagation for tenant/identity.
- **gRPC**: Strong typing, NativeAOT compatible, high performance. Ideal for strict contract boundaries.
- **Message Brokers (RabbitMQ/Kafka)**: Explicitly forbidden by current governance directives to avoid operational complexity unless formally amended.

## 12. Database Boundary
Cells must communicate via APIs. `Core.Cell` cannot directly insert a job into `System.Cell`'s database.

## 13. System.Cell Role
`System.Cell` owns the durable job queue. Asynchronous inter-Cell communication should be implemented by synchronous API calls to `System.Cell`, which then enqueues the durable work.

## 14. Bridge.Cell Role
`Bridge.Cell` will receive Shopify webhooks and API calls, and must delegate synchronization processing to `System.Cell` via the inter-Cell protocol, avoiding direct database manipulation.

## 15. VibeStock Requirements
VibeStock needs inter-Cell communication immediately to connect Commerce logic (`Bridge.Cell`) with Core logic (`Core.Cell`) and Background Processing (`System.Cell`).

## 16. Future Product Reuse
A standardized HTTP or gRPC client architecture, augmented with tenant/identity propagation, will be generically reusable for any future Generated Product Runtime (e.g., Fixed Asset Manager).

## 17. Recommended Architecture
A NativeAOT-compatible HTTP or gRPC protocol should be established as the canonical inter-Cell transport. Synchronous queries will use this transport directly. Asynchronous work will use this transport to submit jobs to `System.Cell`.

## 18. Governance Gaps
- Exact transport choice (HTTP vs gRPC) is undefined.
- Inter-Cell authorization trust model is undefined.

## 19. Required Architecture Amendments
An amendment defining the canonical Inter-Cell Communication Protocol, including context propagation mechanisms, is required before implementation can proceed.

## 20. Implementation Preconditions
No runtime implementation, transport contracts, or SDK modifications may occur until the architecture amendment is approved.
