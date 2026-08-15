# VibeStock User Journey

This document describes the complete end-to-end user journey currently implemented in the VibeStock system. It follows the exact logical flow as validated by the `VibeStockEndToEndCommerceFlowTests` integration test.

## 1. User Input & File Upload
- **INPUT:** A merchant provides a raw product catalog file in CSV or XLSX format.
- **PROCESS:** The file stream is passed into the `ImportEngine`.
- **OUTPUT:** A file stream ready for inspection.
- **POSSIBLE FAILURE:** Malformed file format, unreadable stream, or unsupported file type.

## 2. File Inspection
- **INPUT:** Raw file stream.
- **PROCESS:** `ImportEngine.InspectAsync` reads the file headers and infers the schema.
- **OUTPUT:** A `Schema` object detailing the columns found in the file.
- **POSSIBLE FAILURE:** Empty file or missing headers.

## 3. Column Mapping & AI Suggestions
- **INPUT:** The file schema and the required target fields (`Name`, `Price`, `Sku`, `Inventory`).
- **PROCESS:** `AISemanticMapper.SuggestMappingsAsync` is called. Deterministic rules are applied first (e.g., exact matches). Unmapped columns are sent to the AI Engine for semantic matching (e.g., mapping "Coste" to "Price").
- **OUTPUT:** A list of `ProviderSuggestedMapping` objects with mapping statuses (`Confirmed`, `Suggested`, or `Unresolved`).
- **POSSIBLE FAILURE:** AI timeout, AI unavailability (fallback to `Unresolved`), or ambiguous columns.

## 4. Human Approval
- **INPUT:** The AI suggestions.
- **PROCESS:** The user (or the system acting on their behalf) reviews the `Suggested` mappings and approves them, changing their status to `Confirmed`. The finalized mappings are converted into an import `Configuration`.
- **OUTPUT:** An actionable `Configuration` object.
- **POSSIBLE FAILURE:** The user may reject a mapping or fail to provide a required mapping.

## 5. Data Quality & Product Construction
- **INPUT:** The file stream and the confirmed `Configuration`.
- **PROCESS:** `ImportEngine.ProcessAsync<VibeStockProduct>` parses the file line by line, applying data quality rules (e.g., ensuring `Price` is a valid decimal and `Sku` is not empty).
- **OUTPUT:** An `ImportResult<VibeStockProduct>` containing rows categorized as `Valid` or containing errors.
- **POSSIBLE FAILURE:** Rows with invalid data (e.g., negative prices, missing SKUs) are flagged and excluded from the valid set.

## 6. Product Intelligence & SEO Analysis
- **INPUT:** A valid `VibeStockProduct` row.
- **PROCESS:** `ProductIntelligenceEngine.AnalyzeAsync` evaluates the product details (Title, Description, Tags) using AI. It extracts features, identifies target audiences, determines the tone, generates semantic tags, and identifies content gaps. SEO recommendations and findings are generated as part of this intelligence report.
- **OUTPUT:** A `ProductIntelligenceReport` containing structured intelligence and `SeoFindings`.
- **POSSIBLE FAILURE:** AI unavailability or timeout.

## 7. Commerce / Shopify Abstraction
- **INPUT:** The augmented `VibeStockProduct` (now containing Intelligence and SEO findings).
- **PROCESS:** The product is translated into the `CommerceProduct` abstraction. If there are content gaps, the original description may be modified (e.g., appending tags or enhanced descriptions). Variants, pricing, and inventory are mapped to the generic `CommerceVariant` structure.
- **OUTPUT:** A `CommerceProduct` ready for external synchronization.

## 8. Idempotency & Persistence (Job Processing)
- **INPUT:** The product's SKU (used as an idempotency key).
- **PROCESS:** Before syncing, `IdempotencyEnforcer.TryAcquireAsync` checks the PostgreSQL (Npgsql) store to ensure the product hasn't been synced recently. If acquired, the sync proceeds. Once finished, `CompleteAsync` marks the job as done.
- **OUTPUT:** An `IdempotencyAcquisitionStatus` indicating if the operation is safe to execute.
- **POSSIBLE FAILURE:** Database connectivity issues or duplicate job executions (which are safely ignored).

## 9. Shopify Synchronization
- **INPUT:** The `CommerceProduct`.
- **PROCESS:** `ShopifyProductManager.CreateAsync` translates the abstraction into a Shopify GraphQL mutation (`productCreate`). It handles HTTP rate limiting (429) and temporary failures (500) via a retry handler.
- **OUTPUT:** A `CommerceResult<CommerceProduct>` indicating success or failure.
- **POSSIBLE FAILURE:** Shopify rate limiting (HTTP 429), GraphQL `userErrors` (e.g., validation rules at Shopify's end), or network failures.

## 10. Observability
- **INPUT:** Operations throughout the pipeline.
- **PROCESS:** OpenTelemetry tracks spans and metrics (e.g., AI request durations, GraphQL mutation latencies, and Data Quality failure rates).
- **OUTPUT:** Telemetry data exported via OTLP.
- **POSSIBLE FAILURE:** (Pending external validation) Telemetry backend unreachable.
