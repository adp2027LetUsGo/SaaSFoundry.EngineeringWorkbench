# Architecture Freeze v1.1.2 - Idempotency Persistence Amendment

## 1. Owner & Scope
* **Implementation Owner**: `SaaSFoundry.Plugins.Persistence`
* **Generator Owner**: A new or extended capability within the Persistence Plugin (e.g., `IdempotencyCapability`) generates the implementation.
* **Generated Runtime Owner**: The Generated Product Runtime Cell (under the Persistence subsystem) owns the executing source code.
* **Cell Scope**: Idempotency state strictly honors the **Database-per-Cell** architecture. Each Cell's PostgreSQL database owns the idempotency state for operations mutating its own data.
* **Database Scope**: The state is stored in an infrastructure schema table (e.g. `sys_idempotency_keys`), separate from business domain tables.

## 2. RLS & Tenancy
* **Tenant Scope**: Operations are strictly isolated by Tenant. The composite primary key for idempotency enforcement is `(tenant_id, idempotency_key)`.
* **RLS Interaction**: The idempotency checks run as a **system-level persistence operation**, bypassing standard PostgreSQL RLS policies. This is because the `TryAcquireAsync` check occurs in the gRPC middleware layer *before* the tenant-scoped Background Worker establishes the tenant RLS context.
* **Connection Owner**: The persistence runtime creates and manages a system-level connection for idempotency. The gRPC layer is forbidden from accessing `NpgsqlConnection` or creating its own connection abstraction.

## 3. Persistent State & Atomicity
* **Table Structure**: The infrastructure table must represent the composite key, status, creation timestamp, and TTL expiration.
* **Atomic Acquisition**: Concurrency is resolved via PostgreSQL `INSERT ... ON CONFLICT DO NOTHING` (or atomic conditional `UPDATE` for expired rows). Only one concurrent caller successfully mutates the row to acquire the lock.
* **Statuses**:
  * `Acquired`: Caller obtained ownership via atomic SQL statement.
  * `InProgress`: Row exists but is not completed.
  * `AlreadyProcessed`: Row is marked completed.
* **Crash Recovery & TTL**: An `expires_at` (TTL) timestamp is stored on acquisition. If a key remains `InProgress` past the TTL (e.g., due to a Pod crash before `CompleteAsync`), a subsequent `TryAcquireAsync` can atomically claim it using a conditional `UPDATE` where `expires_at < NOW()`.

## 4. Transaction Boundaries
* **Transaction Owner**: The Persistence plugin generates the implementation. The gRPC layer must not manage PostgreSQL transactions.
* **Decoupled Lifecycles**: `TryAcquireAsync` runs its own autonomous transaction to lock the key. The business operation executes in a separate transaction. `CompleteAsync` runs in another autonomous transaction to mark the key `AlreadyProcessed`. If the business transaction rolls back, `CompleteAsync` is skipped, leaving the key in `InProgress` state until TTL expires.

## 5. Runtime Constraints
* **Dapper.AOT**: Required for all SQL execution.
* **NativeAOT**: Mandatory. No reflection, `dynamic`, or runtime `Type` discovery is allowed in the generated idempotency enforcer.

## 6. Testing Requirements
* Testcontainers PostgreSQL is mandatory to verify concurrency guarantees, unique constraints, cross-tenant isolation, TTL recovery semantics, and Dapper.AOT execution.
