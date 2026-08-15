# ARCHITECTURE FREEZE AMENDMENT PROPOSAL: IDENTITY & TENANCY PLATFORM CONTRACTS

## DECISION ID
AMEND-2026-002

## STATUS
PROPOSAL

## PROBLEM STATEMENT
The architectural gap analysis (Stages 8A/8B) verified the following:
- Authentication requires canonical identity context.
- API requires canonical identity and tenant context.
- Persistence requires canonical tenant context for database isolation.
- PostgreSQL RLS requires a trustworthy tenant context.
- The current frozen Core contracts do not define these primitives.

Without canonical ownership of these domain models in the platform foundation, plugins are forced to invent competing, decentralized contracts, severely compromising architectural integrity and the strict dependency flow.

## MINIMUM CONTRACT SET
The platform requires an explicitly separated contract model to distinguish these responsibilities.

### 1. Identity Contract
- **Purpose:** Represents the authenticated caller's identity (the "Who").
- **Consumers:** Authentication (Producer), API, Authorization, Persistence, Application Code.
- **Lifetime:** Scoped to the execution request/session.
- **Mutability:** Immutable.
- **Security:** Highly sensitive. Must not contain raw tokens or passwords.
- **AOT:** Must be fully AOT-compatible and reflection-free.
- **Fields:** `SubjectId`, `IdentityType` (e.g., User, System, Agent), `Claims` (if canonical), `TenantAssociation`. No raw user profile data (email/name) unless strictly required.

### 2. Tenant Context
- **Purpose:** Represents the tenant boundary for data isolation (the "Where").
- **Consumers:** Persistence (RLS), API.
- **Producer:** Authentication/Identity Resolution.
- **Fields:** `TenantId`. (Does *not* contain connection strings, credentials, or implementation-specific RLS SQL).

### 3. Authentication Context
- **Purpose:** Represents the state and mechanism of the current authentication session.
- **Consumers:** Authorization, API.
- **Fields:** `AuthenticationScheme` (e.g., Bearer, ApiKey), `AuthenticationStatus` (e.g., Authenticated, Expired). Separated from Identity to prevent mixing caller traits with session state.

### 4. Authorization Context
- **Purpose:** Represents the evaluated permissions for the requested operation (the "What").
- **Consumers:** API.
- **Fields:** `Permissions`, `Roles`. (Explicitly separated from Workbench Governance Policy, which governs system agents/plugins, not application users).

## CANONICAL OWNER
**SaaSFoundry.EngineeringWorkbench.Core** is the only structurally sound owner.
- **Why these are platform contracts:** They represent fundamental execution domain concepts shared across all system layers.
- **Why SDK.Core is not the owner:** The SDK provides developer-facing plugin building blocks, not runtime business domain definitions.
- **Why Authentication cannot own them:** API and Persistence must consume these contracts. Having them depend on `SaaSFoundry.Plugins.Authentication` creates tight coupling to a sibling plugin, violating the strict `Plugin -> Core` dependency direction.

## FREEZE AMENDMENT MODEL
**Architecture Freeze v1.0 + Controlled Amendment = Architecture Freeze v1.0.1**

### Allowed Additions
- **Assembly:** `SaaSFoundry.EngineeringWorkbench.Core`
- **Namespace:** `SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity` (and/or Security/Tenancy)
- **Interfaces/Types:** `IIdentityContext`, `ITenantContext`, `IAuthenticationContext`, `IAuthorizationContext`
- **Tests:** Additive unit tests verifying immutability and AOT determinism.

### Forbidden Changes
- Modification, renaming, or deletion of existing Core contracts.
- Modification of existing Governance, Planner, Catalog, Validation, Packaging, or Observability mechanisms.

## DEPENDENCY GRAPH
```text
                 Platform Contracts
                ┌──────────────────┐
                │ Identity         │
                │ TenantContext    │
                │ Auth Context     │
                │ Authz Context    │
                └────────┬─────────┘
                         │
            ┌────────────┼────────────┐
            ▼            ▼            ▼
     Authentication     API      Persistence
                                      │
                                      ▼
                                 PostgreSQL
                                      │
                                      ▼
                                     RLS
```
No cycles are introduced.

## SDK BOUNDARY
The strict invariant `SDK.Core has zero internal dependencies` remains protected. `SDK.Core` MUST NOT depend on `EngineeringWorkbench.Core` merely to consume identity contracts unless explicitly required and authorized by further Canon amendments.

## NATIVE AOT
All added types must be interfaces or immutable records, free of dynamic resolution, runtime type discovery, and assembly scanning.

## TENANCY / RLS BOUNDARY
- **Authentication:** Establishes authenticated identity.
- **Identity/Tenant platform contracts:** Carry trusted contextual information.
- **API:** Transports context into application execution.
- **Persistence:** Consumes `TenantContext`.
- **PostgreSQL:** Enforces tenant isolation through RLS (implementation hidden from TenantContext).

## IMPACT ANALYSIS
- **EngineeringWorkbench.Core:** Requires additive interfaces/records.
- **Authentication (Stage 8):** Blocked until Core is amended. Will produce these contexts.
- **API (Stage 7):** Requires updating to attach contexts to HTTP requests.
- **Persistence (Stage 6):** Requires updating to apply `TenantContext` to `Npgsql` connections/Dapper.
- **SDK/Observability/Testing/Governance:** No required impact.

## CERTIFICATION IMPACT
Persistence (Stage 6) and API (Stage 7) have achieved "STAGE COMPLETE" status. Implementing this amendment will require regressions of both stages, as well as Observability and the broader SDK baseline, prior to final production certification.

## DECISION
**AMENDMENT_REQUIRED**
