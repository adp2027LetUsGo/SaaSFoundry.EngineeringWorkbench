# AHS Correlation Context Specification

Every execution carries:

- ExecutionId
- TenantId
- CorrelationId
- Trace Context

Context must propagate through:

- API calls
- Domain Events
- Background Jobs
- Agent executions
