# VibeStock AI Decision Points

This document explicitly defines the boundaries and use cases where Artificial Intelligence (via `IAIEngine`) is employed within the VibeStock system. Deterministic fallback logic is strictly separated from AI inference.

## 1. Column Mapping (`AISemanticMapper`)
- **Input:** File Schema (extracted column names) and the required Target Fields.
- **Purpose:** To understand non-standard column headers (e.g., "Referencia" -> "Sku", "Coste" -> "Price") when deterministic matching fails.
- **Output:** A `ProviderSuggestedMapping` response.
- **Validation:** The AI is strictly typed to return a `SemanticMappingProviderResponse` structure.
- **Fallback:** If AI fails, times out, or is unavailable, the columns are marked as `Unresolved` and require manual mapping.
- **Human Approval:** **REQUIRED.** All AI-suggested mappings are marked as `Suggested` and must be explicitly approved by a human operator before proceeding. AI cannot autonomously alter the schema configuration.
- **Modifies Canonical State:** No.

## 2. Product Intelligence & SEO (`ProductIntelligenceEngine`)
- **Input:** `ProductIntelligenceRequest` (containing Sku, Title, Description, and Tags).
- **Purpose:** To extract factual information and infer semantic details (Target Audience, Tone, Content Gaps, Semantic Tags) and generate SEO findings from raw product descriptions.
- **Output:** A `ProductIntelligenceReport` formatted as a JSON string and parsed into strongly-typed objects.
- **Validation:** The AI response must deserialize correctly into the expected JSON structure.
- **Fallback:** If AI is unavailable, the `Intelligence` and `SeoFindings` fields remain `null`. The system gracefully continues syncing without the enhanced data.
- **Human Approval:** **NOT REQUIRED.** The insights are generated automatically for valid products.
- **Modifies Canonical State:** The core product properties (Name, Price, SKU) are NEVER modified autonomously. However, if the AI identifies "Content Gaps", the system will append an "[Enhanced]" tag to the description during the commerce abstraction phase. This is an additive enhancement, not a destructive rewrite of the canonical data.
