# VibeStock Production Operations Map

This document outlines the operational and infrastructure architecture required to run VibeStock in a production environment.

## 1. Operational Architecture

### Cells (Microservices)
VibeStock is deployed using a cellular architecture for isolation and scaling:
- **VibeStock.Ingestor.Cell:** Handles the compute-heavy tasks of file parsing, AI mapping orchestration, and initial data quality filtering.
- **VibeStock.System.Cell:** Orchestrates core business logic, including Product Intelligence and SEO evaluations.
- **VibeStock.Bridge.Cell:** Acts as the integration layer interfacing with external systems (Shopify).

### Processes & Jobs
- The system heavily relies on asynchronous background processing. Workloads (like processing a 10,000-row CSV) are split into parallel jobs.
- Workers use idempotency locks on SKUs to ensure jobs can safely restart without duplicating work.

### Databases
- **PostgreSQL (Npgsql):** The primary persistence store. It manages Idempotency locks and operational state.

### AI Provider Boundary
- Connects to an external LLM/AI provider (e.g., OpenAI) via the generic `IAIEngine` interface. Strict network timeouts and fallback logic govern this boundary.

## 2. Configuration & Secrets
- **Shopify Credentials:** Requires an access token (`X-Shopify-Access-Token`) and store URL.
- **AI Credentials:** API keys for the configured AI provider.
- **Database Credentials:** Connection strings for PostgreSQL.
- *Note: Secrets are injected at deployment time and are explicitly excluded from application logs.*

## 3. Telemetry
- **OpenTelemetry:** Configured to emit distributed traces and metrics.
- Currently, local testing verifies spans are created, but it requires an external OTLP-compatible destination (e.g., Jaeger, Prometheus, or Datadog) for production.

## 4. Deployment Requirements
- **NativeAOT:** The application is compiled ahead-of-time (AOT). This requires deployments to support self-contained, trimming-safe binaries (currently targeting `win-x64` or equivalent Linux containers). No dynamic runtime reflection is permitted.

## 5. What Remains External (Pending Validation)
The following elements are explicitly marked as pending Stage 9 external validation:
- **Oracle Deployment:** The system has not been deployed to the target Oracle infrastructure.
- **Real Shopify Credentials:** Integration is currently validated via HTTP mocks. It has not connected to a live Shopify storefront.
- **Production Secrets:** Real keys have not been introduced into the environment.
- **Production Telemetry Destination:** OTLP traffic is currently blackholed/mocked.
