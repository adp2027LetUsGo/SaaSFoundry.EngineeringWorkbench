# VibeStock Data Flow

This document details the movement of data across architectural boundaries.

## 1. Flow Overview

The overall trajectory of catalog data is:
`Ingestor.Cell` ➔ `System.Cell` ➔ `Bridge.Cell` ➔ Shopify

## 2. Ingestor.Cell (Data Intake & Normalization)
- **Inputs:** Raw CSV/XLSX files uploaded by the user.
- **Operations:**
  - File parsing (converting raw streams to dictionaries).
  - Schema extraction.
  - Applying Data Quality rules.
- **AI Boundary:** The `AISemanticMapper` executes here to assist in mapping unmapped columns.
- **Outputs:** An `ImportResult<VibeStockProduct>` containing categorized (Valid/Invalid) domain entities.

## 3. System.Cell (Orchestration & Enhancement)
- **Inputs:** Validated `VibeStockProduct` instances via internal gRPC services.
- **Operations:**
  - Manages the core business logic.
  - Dispatches products to the `ProductIntelligenceEngine`.
- **AI Boundary:** The Product Intelligence workflow executes here, augmenting the product with semantic tags, feature extraction, and SEO findings.
- **Outputs:** Enhanced `VibeStockProduct` entities ready for commerce translation.

## 4. Bridge.Cell (Commerce Synchronization)
- **Inputs:** Enhanced `VibeStockProduct` entities.
- **Operations:**
  - Transforms `VibeStockProduct` into the generic `CommerceProduct`.
  - Enforces Idempotency (preventing duplicate pushes).
  - Translates the generic abstraction into Shopify GraphQL operations.
- **Persistence Boundary:** The `IdempotencyEnforcer` interacts with a PostgreSQL database (Npgsql) to acquire and complete locks based on the product's SKU.
- **External Boundary:** Communicates with Shopify via HTTP Client, utilizing a rate-limited retry handler.
- **Outputs:** `CommerceResult<CommerceProduct>` indicating success or failure.

## 5. Summary of Data Transformations
1. **Raw String Streams** -> parsed to `Dictionary<string, string>`
2. **Dictionary** -> mapped via `Configuration` to `VibeStockProduct`
3. **VibeStockProduct** -> augmented with `ProductIntelligenceReport` and `SeoFinding[]`
4. **VibeStockProduct** -> abstracted to `CommerceProduct` (Description is modified here if content gaps exist)
5. **CommerceProduct** -> translated to Shopify `productCreate` GraphQL payload
