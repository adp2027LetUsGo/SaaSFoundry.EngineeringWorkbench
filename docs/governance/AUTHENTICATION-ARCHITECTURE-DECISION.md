# AUTHENTICATION ARCHITECTURE DECISION

## DECISION ID
AUTH-2026-001

## PROBLEM STATEMENT
Stage 8 requires implementing the `SaaSFoundry.Plugins.Authentication` plugin to populate the canonical frozen platform contracts (`IdentityContext`, `TenantContext`, `AuthenticationContext`, `AuthorizationContext`). However, the repository contains no canonical evidence defining the authentication mechanism for VibeStock and the SaaS platform. A definitive architectural choice must be made before implementation.

## PRODUCT CONTEXT
VibeStock requires an authentication mechanism supporting:
- **Dashboard Users:** Human users interacting with the SaaS UI.
- **SaaS Tenants:** Multi-tenant isolation based on authenticated identity.
- **API Consumers:** External integrations and potential future mobile/client applications.
- **Machine-to-Machine:** Automated services calling the VibeStock API.
- **Shopify Merchant Connections:** Note that Shopify OAuth is a separate integration concern (Commerce domain) and does not govern primary user authentication.

## OPTIONS

### Option A: OIDC / OAuth 2.0 + JWT Bearer
- **Pros:** Stateless, scalable, standard for APIs and SPAs. Ideal for external Identity Providers.
- **Cons:** Harder to revoke instantly without complex token blocklists. Not ideal for direct machine-to-machine without client credentials flow.

### Option B: Cookie-based authentication
- **Pros:** Built-in browser security (HttpOnly, SameSite), easy revocation, traditional for monolithic dashboards.
- **Cons:** Poor fit for mobile APIs, SPA architecture, and machine-to-machine integrations.

### Option B: API Key authentication
- **Pros:** Simple, fast, ideal for machine-to-machine.
- **Cons:** Insecure for human-user sessions in browsers.

### Option D: Hybrid: OIDC/JWT for human users, API Keys for machine-to-machine access
- **Pros:** Solves all product context needs. Humans get standard OIDC flows; machines get easily rotated, scoped keys.
- **Cons:** Slightly more complex to implement and manage dual schemes.

## COMPARISON MATRIX
| Criteria | A: OIDC/JWT | B: Cookie | C: API Key | D: Hybrid |
| :--- | :--- | :--- | :--- | :--- |
| **NativeAOT Compatibility** | 5 | 5 | 5 | 5 |
| **.NET 10 Compatibility** | 5 | 5 | 5 | 5 |
| **Minimal API Integration** | 5 | 5 | 5 | 5 |
| **TenantContext Integration** | 5 | 5 | 5 | 5 |
| **IdentityContext Integration** | 5 | 5 | 5 | 5 |
| **Security** | 4 | 4 | 2 | 5 |
| **Token/Session Lifecycle** | 4 | 5 | 3 | 4 |
| **Multi-tenant SaaS** | 5 | 4 | 3 | 5 |
| **Future API Clients** | 5 | 2 | 4 | 5 |
| **Machine-to-Machine** | 2 | 1 | 5 | 5 |
| **Operational Complexity** | 3 | 4 | 5 | 3 |
| **VibeStock Suitability** | 4 | 2 | 2 | 5 |

*(1 = poor, 3 = acceptable, 5 = strong)*

## RECOMMENDED ARCHITECTURE
**Option D: Hybrid: OIDC/JWT for human users, API Keys for machine-to-machine access**

This architecture natively supports the primary VibeStock personas: human users accessing the SaaS dashboard via JWT Bearer tokens, and programmatic integrations via API Keys.

### Identity Integration
Authentication will resolve the token or key to an `IdentityContext` populated with the `SubjectId`, `IdentityType` (User or Machine), and `Claims`.

### Tenant Integration
**Trusted Tenant Association:** A client-supplied `TenantId` must NEVER automatically become trusted context. The authentication middleware will resolve the authenticated `IdentityContext`, lookup the user's/machine's authorized tenant associations securely, and inject a verified `TenantContext` into the request pipeline.

### Authorization
Application authorization will operate strictly on the `Permissions` and `Roles` populated within the `AuthorizationContext`, decoupling it entirely from EngineeringWorkbench system Governance.

### API Integration
The API plugin will utilize standard ASP.NET Core authentication middleware configured for dual schemes (Bearer + ApiKey). The middleware will execute early in the request pipeline, producing the canonical Core contexts for subsequent endpoints.

### Machine-to-Machine Strategy
API Keys will be utilized. They will carry restricted scopes and map directly to specific tenant associations to ensure secure, non-interactive integrations.

### Shopify Separation
VibeStock User Authentication ≠ Shopify OAuth. The authentication mechanism defined here protects the SaaS platform. Shopify OAuth is a downstream integration mechanism belonging to `SaaSFoundry.Plugins.Commerce` (Stage 10) for authorizing API calls to Shopify on behalf of merchants.

### AOT Implications
The hybrid model utilizes `Microsoft.AspNetCore.Authentication.JwtBearer` and custom API Key handlers, which are natively supported by ASP.NET Core Minimal APIs and NativeAOT. All canonical contracts remain immutable records/interfaces requiring zero reflection.

### Implementation Requirements
- Do not modify existing Core contracts.
- Configure ASP.NET Core Authentication in the new Authentication plugin.
- Implement token validation and API key validation.
- Implement secure Identity-to-Tenant resolution.

## DECISION: PROPOSED
