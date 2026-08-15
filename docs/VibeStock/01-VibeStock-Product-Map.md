# VibeStock Product Map

## 1. What VibeStock Is
VibeStock is an automated commerce catalog ingestion and enhancement engine built upon the SaaSFoundry architecture. It acts as an intelligent conduit between raw merchant data files (CSV, XLSX) and the Shopify storefront, ensuring that product catalogs are not only imported but enriched, verified, and mapped correctly before synchronizing with Shopify.

## 2. Who Uses It
VibeStock is designed for merchants, catalog managers, and commerce operators who need to bulk-import large, often unstructured or inconsistent product catalogs into Shopify while ensuring high data quality and enhanced product information (SEO, structured metadata).

## 3. What Problem It Solves
Traditional catalog imports are brittle: they fail on bad data, require exact column names, and simply push raw data without enriching it. VibeStock solves this by providing:
- **Intelligent Ingestion:** Accepts varied formats and maps columns using AI.
- **Data Quality:** Validates constraints before touching the live store.
- **Product Intelligence & SEO:** Automatically extracts structured data, tags, and SEO findings from raw descriptions.
- **Robust Synchronization:** Uses idempotency, persistence, and background jobs to safely sync to Shopify via GraphQL.

## 4. Current Product Capabilities (Implemented)
- **Import Engine:** Parses CSV and XLSX files.
- **Interactive Mapping:** Supports AI-assisted column mapping and human approval workflows.
- **Data Quality:** Rules-based validation (SKU presence, price constraints, etc.).
- **Product Intelligence:** AI-driven extraction of semantic information (e.g., tags, descriptions).
- **SEO Analysis:** Deterministic and AI-driven SEO findings.
- **Commerce / Shopify Abstraction:** Translates the generic `VibeStockProduct` domain model into Shopify GraphQL operations.
- **Background Processing:** Idempotent job execution backed by PostgreSQL (Npgsql) with NativeAOT compilation.
- **Observability:** OpenTelemetry integration.

## 5. Main Boundaries
- **Ingestor.Cell:** Handles file import, mapping, and data quality.
- **System.Cell:** Orchestrates Product Intelligence, SEO, and business logic.
- **Bridge.Cell:** The adapter layer that translates and syncs data to the external Shopify system.
- **External Dependencies:** OpenAI (or generic AI provider), PostgreSQL, Shopify GraphQL API.

## 6. Current Implementation Status

### IMPLEMENTED
- File Upload & Parsing (CSV, XLSX)
- AI-Assisted Column Mapping
- Data Quality Engine
- Product Domain Model (`VibeStockProduct`, Variants, Pricing, Inventory)
- Product Intelligence Engine (extraction of structured descriptions/tags)
- SEO Analysis Engine
- Background processing, Persistence, Idempotency
- Shopify GraphQL adapter and Commerce abstraction

### PLANNED / FUTURE
- Frontend / User Interface
- Speculative marketing features
- Oracle Deployment (Next phase)

### EXTERNAL VALIDATION PENDING
- Real external Shopify deployment and validation
- Real Oracle infrastructure deployment
- Production credentials / telemetry destinations
