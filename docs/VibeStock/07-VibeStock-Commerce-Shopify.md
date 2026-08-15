# VibeStock Commerce & Shopify Integration

This document outlines how VibeStock integrates with the Shopify platform.

## 1. Commerce Abstraction
To avoid tightly coupling the VibeStock orchestration engine to a specific storefront, the system relies on the `SaaSFoundry.SDK.Commerce` abstraction. The generic `CommerceProduct` entity is passed to an `ICommerceProductManager` which handles the platform-specific translation and synchronization.

## 2. Shopify Adapter
The Shopify adapter (`ShopifyProductManager`) implements the `ICommerceProductManager` interface. It translates generic `CommerceProduct` properties (Title, Description, Vendor, Variants) into Shopify-specific GraphQL payloads.

## 3. Implemented GraphQL Operations
- **`productCreate`:** A GraphQL mutation to create new products in Shopify.
- *Note:* Product Updates, Variants specific endpoints, and deletions (`GetBySkuAsync`, etc.) are defined in the interface but currently throw "Not implemented yet" or are stubs.

## 4. Authentication Boundary
Authentication is handled via the underlying `HttpClient` initialized during adapter setup. The pipeline expects proper authentication headers (e.g., `X-Shopify-Access-Token`) to be configured prior to instantiating the HTTP client.

## 5. Rate Limiting & Retry
The Shopify API enforces strict rate limits. The system uses a `ShopifyRateLimitHandler` wrapped around the HTTP client to:
- Automatically intercept `HTTP 429 Too Many Requests` responses.
- Apply an exponential backoff retry strategy.
- Limit the maximum number of retries to prevent infinite blocking.

## 6. Error Handling (`userErrors`)
Shopify's GraphQL API returns a HTTP 200 even for domain-level validation failures. The adapter explicitly parses the response payload to check for:
- Top-level GraphQL syntax/server errors.
- Domain validation failures inside the `productCreate.userErrors` array.
These are safely translated back to the generic `CommerceErrorType.Validation` status.

## 7. Idempotency
To prevent duplicate products from being created during worker restarts or transient network failures, operations are guarded by the `IIdempotencyEnforcer`.
- A lock is requested based on the product's SKU (e.g., `sync_SKU-123`).
- If the lock is `AlreadyProcessed`, the operation is safely skipped (returning `CommerceErrorType.Conflict`).
- If successful, the idempotency lock is permanently marked as completed via `CompleteAsync`.

## 8. External Validation Status
### LOCAL / MOCK VALIDATION: COMPLETE
The Shopify adapter is fully validated against local mocks simulating HTTP 429s, HTTP 500s, and standard `productCreate` responses.

### REAL SHOPIFY VALIDATION: PENDING
The system has NOT been tested against a live Shopify storefront with real credentials. The current functionality relies entirely on integration tests acting as the external boundary.
