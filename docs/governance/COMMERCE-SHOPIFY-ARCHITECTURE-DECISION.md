# COMMERCE / SHOPIFY ARCHITECTURE DECISION

## 1. Commerce Boundary
Commerce / Shopify is a VibeStock-specific product integration, not a generic SaaSFoundry capability. It executes within `VibeStock.Bridge.Cell`.

## 2. Shopify Boundary
The integration spans Shopify OAuth installation, Shopify GraphQL API consumption, Webhooks processing, and potential synchronization. 

## 3. Bridge.Cell Ownership
`VibeStock.Bridge.Cell` is the exclusive owner of Shopify integration. No other Cell will interface with Shopify directly.

## 4. Platform Authentication vs Shopify Authentication
Platform Authentication (`SaaSFoundry.Plugins.Authentication`) protects the SaaS APIs. Shopify Authentication (OAuth access tokens) belongs exclusively to `Bridge.Cell`'s Commerce scope. Shopify tokens must not leak into generic `IdentityContext` or `AuthorizationContext`.

## 5. Tenant Model
Each Shopify connection maps to a specific Platform Tenant. The `TenantContext` must enforce isolation for all Shopify state.

## 6. Persistence Boundary
`Bridge.Cell` persists Shopify data inside its own PostgreSQL database, adhering to the Database-per-Cell architecture. Cross-cell database access is strictly forbidden.

## 7. BackgroundProcessing Integration
Shopify synchronization tasks and webhook ingestion will leverage the existing certified BackgroundProcessing generator (`IJobStorageCapability`).

## 8. Observability Integration
Shopify calls will be traced using existing observability. Sensitive data (Tokens, API Keys, Secrets, PII payloads) must never be logged.

## 9. AOT Constraints
The entire integration must remain strictly NativeAOT compatible. `System.Reflection`, `dynamic`, and `Activator.CreateInstance` remain strictly forbidden.

## 10. Factory v1.0 Classification
Commerce/Shopify is completely outside Factory v1.0 scope. Factory v1.0 is strictly for generic platform plugins.

## 11. VibeStock-specific Boundary
All Shopify GraphQL types, AI mappings, and normalizations belong explicitly to the VibeStock product scope, not the platform Factory.

## 12. Certification Status
**BLOCKED**

## 13. Unresolved Governance Gaps (Architecture Gap)
- **Shopify Contracts Missing**: No canonical evidence exists in the repository for Shopify GraphQL integration, Shopify Authentication/OAuth, or Webhooks.
- **Inter-Cell Protocol Missing**: There is no defined canonical mechanism for `VibeStock.Bridge.Cell` to communicate with `Core.Cell` or `Ingestor.Cell`.
- **Shopify API/Throttling Definition**: No canonical definition exists for handling Shopify's specific rate limiting (e.g., Leaky Bucket GraphQL costs).

No implementation can proceed without violating the "Do NOT invent runtime semantics" rule.
