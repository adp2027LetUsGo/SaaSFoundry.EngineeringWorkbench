# Logging Architecture

Document ID: OBS-002

Version: 0.1

Status: Draft


## Purpose

Define the standard logging architecture for SaaS-Foundry.


## Logging Framework

Standard:

Microsoft.Extensions.Logging


Reasons:

- .NET native integration
- Native AOT compatibility
- source generated logging support


## Logging Format

Required:

Structured JSON


Each log event must include:

- Timestamp
- Level
- Category
- Message
- TraceId
- SpanId
- TenantId
- CellId
- ExecutionId


## Logging Levels

Trace

Detailed diagnostic information.


Debug

Development troubleshooting.


Information

Normal operational events.


Warning

Recoverable issues.


Error

Failed operations.


Critical

System failures.


## Logging Rules

Do not log:

- passwords
- tokens
- secrets
- personal sensitive information


## Enrichment

Logs must support:

- request context
- tenant context
- execution context
- agent context


## Implementation

Preferred approach:

LoggerMessage source generators.


## Status

Draft
