# IDENTITY AND TENANT CONTRACT DECISION

## DECISION ID
DEC-2026-001

## STATUS
UNRESOLVED

## PROBLEM
The Architecture requires canonical contracts for `Identity`, `TenantContext`, `AuthenticationContext`, and `AuthorizationContext` to support the Authentication plugin, API endpoints, and Persistence (PostgreSQL RLS). However, these contracts currently do not exist in the canonical codebase.

## AFFECTED COMPONENTS
* `SaaSFoundry.Plugins.Authentication` (Cannot be built without contracts)
* `SaaSFoundry.Plugins.API` (Requires Identity/Tenant context for HTTP requests)
* `SaaSFoundry.Plugins.Persistence` (Requires Tenant context for DB isolation/RLS)
* `SaaSFoundry.EngineeringWorkbench.Core` (Potential owner)
* `SaaSFoundry.SDK.Core` (Potential owner)

## EXISTING CANONICAL EVIDENCE
As verified in Stage 8A, there are zero existing mentions of `TenantContext`, `Authentication`, `IdentityContext`, or `RLS` in the `SaaSFoundry.EngineeringWorkbench.Core` contracts or the `/docs/canon` documentation. The only security-related contract is `RequiredPermissions`, which governs agent execution, not caller identity or multi-tenancy.

## OPTIONS EVALUATED

### OPTION A: EngineeringWorkbench.Core
* **Concept:** Place identity and tenant primitives in the foundational Core contracts assembly.
* **Dependency Consequences:** API, Persistence, and Authentication would all continue to depend on Core. No cyclical dependencies introduced.
* **Freeze Consequences:** Modifying `SaaSFoundry.EngineeringWorkbench.Core` to add missing identity primitives constitutes a direct modification of the strict Architecture Freeze v1.0.

### OPTION B: SaaSFoundry.SDK.Core
* **Concept:** Place identity and tenant primitives inside the SDK Core.
* **Dependency Consequences:** Plugins would depend on SDK Core to get tenant definitions. 
* **Freeze Consequences:** Does not violate the Core freeze. However, this conflates developer-facing plugin authoring abstractions (SDK) with runtime business/platform domains (Multi-tenancy/Identity). 

### OPTION C: Dedicated SDK Identity/Security layer
* **Concept:** Create a new assembly (e.g., `SaaSFoundry.SDK.Identity.Contracts`).
* **Dependency Consequences:** API, Persistence, and Authentication would depend on this new assembly. 
* **Freeze Consequences:** Avoids modifying `EngineeringWorkbench.Core`. However, Identity and Tenant are fundamental platform abstractions, not SDK build utilities. Ejecting platform domain models into an SDK side-layer creates architectural fragmentation.

### OPTION D: Existing canonical layer
* **Concept:** Use an existing layer.
* **Result:** Rejected. No such layer exists.

## REQUIRED CONTEXTS
* **Persistence:** Requires `TenantId` for PostgreSQL RLS and Database-per-Cell isolation.
* **API:** Requires `IdentityContext` (Authentication) and `TenantContext` to distinguish incoming requests for downstream plugins.
* **Authentication:** Must produce a strongly typed authenticated `Subject`, `TenantContext`, `Claims/Permissions`, and `AuthenticationScheme` without embedding product-specific logic.

## SDK BOUNDARY
Identity and Tenant Context are fundamental runtime platform domain contracts, not developer SDK abstractions. Therefore, they logically do not belong in SDK.Core.

## ARCHITECTURE FREEZE DETERMINATION
Adding fundamental identity/tenant platform contracts to `SaaSFoundry.EngineeringWorkbench.Core` undeniably alters the frozen canonical foundation. The System Canon does not provide explicit authorization to bypass the freeze for missing platform primitives.

## DECISION
**UNRESOLVED**.

Because `EngineeringWorkbench.Core` is structurally the correct canonical owner for platform-wide domain models, but is strictly locked under Architecture Freeze v1.0, a deadlock exists. The Canon provides no resolution for retroactively injecting missing multi-tenancy models into a frozen Core without invalidating the freeze.

## REQUIRED FOLLOW-UP
Architectural Authority must explicitly authorize an exception to the Architecture Freeze v1.0 to allow the introduction of `TenantContext` (and related primitives), or explicitly charter Option C as a valid platform extension pattern.
