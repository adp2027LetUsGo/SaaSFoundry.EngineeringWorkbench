# OBS-003 â€” Logging Standards

Version: 1.0
Status: Canon

---

# Objective

Define uniform logging behavior across the SaaS-Foundry platform.

Logs are operational evidence.

Logs are not debugging notes.

Logs must support autonomous reasoning.

---

# Principles

Structured

Consistent

Deterministic

Correlated

Searchable

Minimal

High Value

Machine Readable

---

# Log Levels

Trace

Debug

Information

Warning

Error

Critical

None

---

# Structured Logging

Every log entry should expose structured properties instead of formatted text.

Preferred fields include:

Timestamp

Level

Message

Category

EventId

CorrelationId

TraceId

SpanId

TenantId

CellId

ExecutionId

WorkflowId

Host

Environment

Version

---

# Categories

Runtime

Execution

Workflow

Messaging

Database

Infrastructure

Security

Integration

Scheduler

BackgroundJob

API

Authentication

Authorization

Deployment

AI

Observability

---

# Logging Rules

Log meaningful events.

Avoid repetitive noise.

Avoid duplicated messages.

Avoid ambiguous wording.

Avoid hidden context.

Prefer structured properties.

Avoid excessive serialization.

---

# Exceptions

Exceptions should capture:

Type

Message

Stack

Source

Inner Exception

Operation

Correlation

Execution Context

---

# Sensitive Information

Logs must never expose:

Secrets

Passwords

Tokens

Private Keys

Personal Data

Credential Material

Sensitive Headers

Protected Payloads

---

# Correlation

Every log should participate in distributed correlation whenever possible.

Primary identifiers include:

CorrelationId

TraceId

SpanId

ExecutionId

WorkflowId

TenantId

CellId

---

# Performance

Logging must not significantly affect execution latency.

Instrumentation should remain asynchronous whenever practical.

Batching and buffering are runtime responsibilities.

---

# Operational Value

Logs should help answer:

What happened?

Where?

When?

Why?

Who initiated it?

Which dependency failed?

Which retry occurred?

Which workflow changed?

Which Cell emitted it?

Which deployment produced it?

