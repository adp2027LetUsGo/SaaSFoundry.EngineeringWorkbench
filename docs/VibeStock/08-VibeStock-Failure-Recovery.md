# VibeStock Failure & Recovery

This document describes how the VibeStock system behaves under various failure conditions, based on the actual implemented behavior.

## 1. Import Failures
- **Invalid/Malformed File:** The `ImportEngine` will fail to parse the stream during the inspection phase, immediately aborting the import job.
- **Invalid Mapping:** If a user submits an incomplete `Configuration`, or if AI mapping fails (`Unresolved`), the system will flag the mapping as incomplete and require manual intervention before proceeding to Data Quality.

## 2. AI Failures
- **AI Unavailable/Timeout:** 
  - *During Mapping:* The column status falls back to `Unresolved`, requiring human mapping.
  - *During Product Intelligence:* The system gracefully catches the unavailability. The `Intelligence` and `SeoFindings` fields remain `null`, allowing the product to still sync to Shopify without the enhanced data.
- **AI Validation Failure:** If the AI returns malformed JSON or hallucinated structures, it is treated identically to an AI timeout/unavailability.

## 3. Data Quality Failures
- **Row-level Constraint Failure:** If a product row fails a data quality check (e.g., negative price, missing SKU), the system marks the specific `ImportRecord` as `Invalid`.
- **Recovery:** The pipeline does not crash. Valid rows continue processing to Shopify, while invalid rows are collected in the `ImportResult` for the user to review and correct later.

## 4. Commerce & Shopify Failures
- **HTTP 429 (Too Many Requests):** The `ShopifyRateLimitHandler` intercepts the 429 status and applies an exponential backoff strategy, retrying up to 3 times before failing the operation.
- **HTTP 5xx (Server Errors):** Handled identically to rate limits, with bounded retries.
- **GraphQL `userErrors`:** If Shopify rejects the payload due to domain rules, the `ShopifyProductManager` captures the error message and fails the specific product synchronization permanently. The idempotency lock is NOT completed, meaning a corrected payload can be retried later.

## 5. Infrastructure Failures
- **Database Failure:** If PostgreSQL is unreachable during Idempotency checks, an exception is thrown. The background job orchestrator (which hosts the pipeline) will mark the job as failed and automatically retry it based on its standard retry policies.
- **Duplicate Operation (Worker Restart):** If a worker crashes mid-sync and restarts, it will attempt to process the row again. The `IdempotencyEnforcer` checks the SKU. If it was already completed before the crash, it returns `AlreadyProcessed`, and the system safely skips the Shopify API call to prevent duplication.
