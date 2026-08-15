# VibeStock End-to-End Workflows

This document outlines the major orchestrated workflows within VibeStock.

## 1. Import Workflow

**Trigger:** A user provides a CSV or XLSX file containing raw product data.

1. **Inspection (`ImportEngine.InspectAsync`):** The file is parsed to extract headers and infer the schema.
2. **Mapping (`AISemanticMapper.SuggestMappingsAsync`):** The system attempts to map the file's columns to VibeStock's expected fields. Deterministic matches are resolved immediately. Ambiguous columns are sent to the AI Engine for semantic matching.
3. **Approval:** The user reviews the AI's `Suggested` mappings and confirms them. This finalizes a `Configuration`.
4. **Data Quality (`ImportEngine.ProcessAsync`):** The file is processed row-by-row against the `Configuration`. Rows failing constraints (e.g., negative prices, empty SKUs) are marked as invalid.
5. **Product Construction:** Valid rows are materialized into `VibeStockProduct` instances.

## 2. AI Workflow

**Trigger:** Any operation requiring semantic understanding (e.g., Column Mapping, Product Intelligence).

1. **Input:** Raw data (e.g., unmapped column names, or product descriptions).
2. **Deterministic Analysis:** The system first attempts to resolve the request deterministically.
3. **AI When Required:** If deterministic logic fails, a strongly-typed `AIRequest` is dispatched to the `IAIEngine`.
4. **Validation:** The AI response is parsed and validated (e.g., ensuring a JSON response conforms to expected structures like `ProductIntelligenceReport`).
5. **Human Approval / Confirmation:** For high-risk operations like Column Mapping, the AI output is marked as `Suggested` and strictly requires human confirmation. For Product Intelligence, the output is appended to the product representation directly.

## 3. Product Intelligence Workflow

**Trigger:** A valid `VibeStockProduct` is prepared for synchronization.

1. **Analysis:** The product's SKU, Title, Description, and Tags are packaged into a `ProductIntelligenceRequest`.
2. **Extraction:** The `ProductIntelligenceEngine` requests the AI to extract factual data (e.g., dimensions, features) from the raw text.
3. **Inference:** The AI infers semantic information (Target Audience, Tone, Semantic Tags).
4. **Recommendations:** The AI identifies Content Gaps and suggests improvements. This structured data is appended to the `VibeStockProduct.Intelligence` property.

## 4. SEO Workflow

**Trigger:** The Product Intelligence workflow completes.

1. **Analysis:** The `ProductIntelligenceEngine` automatically generates SEO insights based on the provided product data and inferred features.
2. **Findings:** Deterministic rules and AI analysis surface SEO issues (e.g., missing meta tags, poor keyword density).
3. **Recommendations:** The findings are returned within the `SeoFindings` array on the `VibeStockProduct`.
*Note: The current implementation appends " [Enhanced]" to descriptions if content gaps are found, but does not autonomously rewrite the canonical descriptions in a destructive manner.*

## 5. Commerce Workflow

**Trigger:** The `VibeStockProduct` is fully validated, mapped, and enhanced.

1. **Abstraction:** The `VibeStockProduct` is transformed into a generic `CommerceProduct`.
2. **Idempotency Check:** The system queries the PostgreSQL database using the product's SKU to ensure the sync hasn't already occurred.
3. **Shopify Adapter:** The `CommerceProduct` is passed to the `ShopifyProductManager`.
4. **GraphQL Operations:** The adapter translates the generic product into a `productCreate` GraphQL mutation.
5. **Response/Error Handling:** The adapter handles HTTP responses, managing rate limits (HTTP 429), server errors (HTTP 500), and GraphQL `userErrors`.
6. **Persistence:** Upon a successful `IsSuccess` result, the idempotency lock is completed, persisting the state in the database.
