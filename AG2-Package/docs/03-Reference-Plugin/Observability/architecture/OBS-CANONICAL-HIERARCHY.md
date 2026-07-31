# OBS-CANONICAL-HIERARCHY.md

# SaaS-Foundry Observability Canonical Hierarchy

Document ID:
OBS-HIERARCHY-001

Version:
1.0

Status:
Canon

Scope:
SaaS-Foundry Observability Documentation

Audience:
- Platform Architects
- Human Operators
- Antigravity Agents
- Engineering Teams

---

# 1. Purpose

This document defines the official documentation authority hierarchy
for the SaaS-Foundry Observability Architecture.

Its purpose is to ensure that every architectural topic has a single
authoritative source while preserving historical and reference
documentation.

---

# 2. Documentation Hierarchy

Observability documentation is organized into five authority levels.

Authority always flows from higher levels to lower levels.

```
Founder Constitution
        │
        ▼
Architecture Canon
        │
        ▼
Observability Canon
        │
        ▼
Implementation Guides
        │
        ▼
Operational Guides
```

---

# 3. Level 0 – Platform Authority

Platform-wide governance documents.

Examples:

Founder Constitution

Master Anchor

Engineering Standards

Architecture Book

Blueprint

These documents define platform principles that every architecture,
including Observability, must follow.

---

# 4. Level 1 – Observability Canon

These are the official architecture documents.

They define the architecture itself.

Every future implementation must conform to these documents.

Canonical documents:

OBS-000 Observability Foundation

OBS-001 Observability Architecture

OBS-002 Telemetry Model

OBS-003 Logging Standards

OBS-004 Metrics Architecture

OBS-005 Audit Evidence Architecture

OBS-006 Agent Diagnostics

OBS-007 Telemetry Architecture

OBS-008 Operational Dashboards

OBS-009 Observability Standards

OBS-010 .NET 10 Implementation Guide

OBS-011 Observability Reference Architecture

OBS-012 Cell Observability Contract

OBS-013 Telemetry Storage Strategy

OBS-014 Observability Security Model

OBS-015 Observability Governance Model

OBS-016 Observability Lifecycle Management

OBS-017 Observability Validation Framework

OBS-018 Observability Operational Runbook

---

# 5. Level 2 – Reference Documents

Reference documents expand canonical concepts.

They provide:

- implementation guidance
- examples
- operational recommendations
- technology-specific information

Reference documents:

00-observability-overview.md

01-observability-principles.md

02-logging-standard.md

03-distributed-tracing-standard.md

04-metrics-standard.md

05-audit-ledger-specification.md

06-correlation-context-specification.md

07-cell-observability-contract.md

08-opentelemetry-implementation.md

09-dotnet10-native-aot-guidelines.md

10-observability-reference-architecture.md

11-antigravity-agent-instructions.md

Reference documents never replace canonical architecture.

---

# 6. Level 3 – Governance Documents

Governance documents control documentation itself.

Examples:

OBS-CURRENT-STATE.md

OBS-DOCUMENT-GENERATION-MANIFEST.md

OBS-DOCUMENT-GENERATION-TRACEABILITY.md

OBS-DUPLICATE-RESOLUTION-REGISTER.md

OBS-SCRIPT-GENERATION-STANDARDS.md

These documents define process rather than architecture.

---

# 7. Level 4 – Historical Documents

Historical documents exist only to preserve generation history.

Historical documents include:

OBS-000-observability-architecture-index.md

OBS-001-observability-principles.md

OBS-002-logging-architecture.md

OBS-003-distributed-tracing-architecture.md

Historical documents shall not be used as architectural authority.

---

# 8. Authority Rules

When multiple documents discuss the same topic:

Rule 1

Canonical OBS documents always prevail.

Rule 2

Reference documents may expand but never redefine architecture.

Rule 3

Governance documents define process only.

Rule 4

Historical documents preserve traceability only.

Rule 5

No implementation may contradict canonical architecture.

---

# 9. Conflict Resolution

When documentation conflicts exist:

Step 1

Consult this hierarchy.

Step 2

Consult the Traceability document.

Step 3

Consult the Duplicate Resolution Register.

Step 4

Apply the canonical document.

Step 5

Update supporting documentation if necessary.

---

# 10. Antigravity Rules

Before using any Observability document:

Verify:

- canonical status
- document lineage
- hierarchy level

Agents shall never infer authority based solely on file names.

---

# 11. Future Documentation

New Observability documents shall be classified as one of:

- Canonical
- Reference
- Governance
- Historical

Every new document shall identify:

- authority level
- owner
- purpose
- relationship to existing documentation

---

# 12. Canonical Principle

One architectural topic.

One canonical document.

Many supporting documents.

Zero ambiguity.

---

End of Document