# ARCHITECTURE FREEZE AMENDMENT: mTLS CELL IDENTITY v1.0.8

## 1. Purpose
Resolves the canonical mTLS Cell certificate identity representation missing in Architecture Freeze v1.0.7, enabling secure implementation of the inter-Cell transport foundation.

## 2. Relationship to v1.0.7
This amendment extends v1.0.7 by explicitly defining the SAN mapping format for Cell identity. It does not introduce any new Core contracts or code.

*(Note: The `IIdempotencyEnforcer` contract originally defined in v1.0.7 was replaced by a concurrency-safe atomic version in the [Architecture Freeze v1.0.9 Amendment](ARCHITECTURE-FREEZE-AMENDMENT-IDEMPOTENCY.v1.0.9.md))*
*(Note: System.Cell Job Queue RLS semantics and Generated Runtime Hosting Model are defined in the [Architecture Freeze v1.1.0 Amendment](ARCHITECTURE-FREEZE-AMENDMENT-GRPC-RLS-HOSTING.v1.1.0.md))*

## 3. Approved Cell Trust
mTLS is the canonical mechanism for internal Cell trust.

## 4. Canonical Certificate Identity
The canonical Cell identity representation is the **URI Subject Alternative Name (URI SAN)** within the mTLS client certificate.

## 5. URI Format
The canonical identity MUST follow the exact SPIFFE format:
`spiffe://saasfoundry/<product>/<cell>`

Where:
- **scheme**: `spiffe`
- **authority**: `saasfoundry`
- **path**: `<product>/<cell>`

## 6. VibeStock Cell Identities
The exact, canonical VibeStock identities are mapped 1:1 as follows:
- `spiffe://saasfoundry/vibestock/core` (Core.Cell)
- `spiffe://saasfoundry/vibestock/ingestor` (Ingestor.Cell)
- `spiffe://saasfoundry/vibestock/bridge` (Bridge.Cell)
- `spiffe://saasfoundry/vibestock/system` (System.Cell)

## 7. Non-Canonical Formats
- **Common Name (CN)**: Not canonical. Must not be used as the primary Cell identity.
- **DNS SAN**: Not canonical for Cell identity. Must not be trusted over URI SAN.

## 8. Separation of Identities
- **Cell Identity**: Represents the workload/Cell (the URI SAN).
- **Platform Identity**: Represents the user/machine (`IdentityContext`).
- **Tenant Context**: Represents the data isolation boundary (`TenantContext`).
- **Authorization Context**: Represents evaluated permissions (`AuthorizationContext`).

## 9. Validation Rules
The destination Cell MUST independently validate:
1. The certificate chain terminates in the configured trusted CA.
2. The certificate is currently valid.
3. The certificate contains a URI SAN.
4. The URI SAN uses the canonical `spiffe://` scheme.
5. The URI authority is exactly `saasfoundry`.
6. The product component matches the destination's configured product runtime.
7. The Cell component corresponds to a recognized Cell.
8. The caller Cell is authorized for the requested communication direction.

## 10. Communication Authorization
mTLS authentication identifies the workload but DOES NOT automatically imply authorization.
The allowed graph must be enforced:
- Core.Cell ↔ Ingestor.Cell
- Core.Cell ↔ Bridge.Cell
- Core.Cell → System.Cell
- Ingestor.Cell → System.Cell
- Bridge.Cell → System.Cell

## 11. Certificate Rotation
Rotation is an infrastructure concern. Rotation MUST preserve the canonical URI SAN identity (i.e. replacing the cryptographic material while maintaining the exact `spiffe://` URI).

## 12. Core Ownership
No Core contracts are required. mTLS validation remains exclusively within the Generated Product Runtime infrastructure.
