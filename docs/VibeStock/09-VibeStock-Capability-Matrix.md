# VibeStock Capability Matrix

| Capability | Current Status | Evidence |
|------------|----------------|----------|
| **Import** | IMPLEMENTED | `ImportEngine.InspectAsync` and `ProcessAsync` supporting CSV and XLSX formats. |
| **Data Quality** | IMPLEMENTED | Row-level categorization filtering out invalid products during import processing. |
| **AI Mapping** | IMPLEMENTED | `AISemanticMapper.SuggestMappingsAsync` leveraging `IAIEngine`. |
| **Human Approval** | IMPLEMENTED | `ProviderSuggestedMapping.Status` transitioning from `Suggested` to `Confirmed`. |
| **Product Intelligence** | IMPLEMENTED | `ProductIntelligenceEngine.AnalyzeAsync` injecting features and tone into `VibeStockProduct.Intelligence`. |
| **SEO** | IMPLEMENTED | `SeoFindings` array populated on the product as part of the PI report. |
| **Product management** | IMPLEMENTED | Core domain abstractions (`CommerceProduct`) exist and are successfully managed via adapters. |
| **Variants** | IMPLEMENTED | Basic variant flattening implemented (`CommerceVariant` mapped from `VibeStockProduct` price/inventory). |
| **Inventory** | IMPLEMENTED | Handled directly on `CommerceVariant.InventoryQuantity`. |
| **Pricing** | IMPLEMENTED | Handled directly on `CommerceVariant.Price`. |
| **Commerce** | IMPLEMENTED | Generic `ICommerceProductManager` interface implemented. |
| **Shopify** | IMPLEMENTED (Local Validation) | `ShopifyProductManager` implemented; external real-world deployment pending. |
| **Background Processing** | IMPLEMENTED | Idempotency enforcer handling duplicate job protection. |
| **Observability** | IMPLEMENTED | OpenTelemetry integration configured on pipeline boundaries. |
| **Authentication** | IMPLEMENTED | HTTP Client headers ready; integration tests assert Token absence in logs. |
| **Persistence** | IMPLEMENTED | Idempotency locks utilizing the underlying PostgreSQL store. |
| **Marketing** | NOT IMPLEMENTED | Explicitly excluded from the current scope. |
| **Oracle Deployment** | PENDING | Awaiting Stage 9 external validation. |
