# OBS-DOCUMENT-MAPPING-MATRIX.md

# SaaS-Foundry Observability Document Mapping Matrix

Document ID:
OBS-MAPPING-001

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

This document defines the authoritative mapping between
Observability architecture topics and the documents that govern them.

Each architectural subject shall have one canonical owner.

Supporting documents expand the canonical definition but shall never
replace it.

Historical documents preserve traceability only.

---

# 2. Document Classification

Document categories:

- Canonical
- Reference
- Governance
- Historical

---

# 3. Canonical Mapping Matrix

| Architecture Topic | Canonical Document | Supporting Documents | Historical Documents |
|--------------------|-------------------|----------------------|----------------------|
| Observability Foundation | OBS-000 Observability Foundation | 00-observability-overview.md | OBS-000-observability-architecture-index.md |
| Observability Architecture | OBS-001 Observability Architecture | 01-observability-principles.md | OBS-001-observability-principles.md |
| Telemetry Model | OBS-002 Telemetry Model | 08-opentelemetry-implementation.md | — |
| Logging | OBS-003 Logging Standards | 02-logging-standard.md | OBS-002-logging-architecture.md |
| Metrics | OBS-004 Metrics Architecture | 04-metrics-standard.md | — |
| Audit Evidence | OBS-005 Audit Evidence Architecture | 05-audit-ledger-specification.md | — |
| Agent Diagnostics | OBS-006 Agent Diagnostics | 11-antigravity-agent-instructions.md | — |
| Telemetry Architecture | OBS-007 Telemetry Architecture | 08-opentelemetry-implementation.md | OBS-003-distributed-tracing-architecture.md |
| Operational Dashboards | OBS-008 Operational Dashboards | 00-observability-overview.md | — |
| Observability Standards | OBS-009 Observability Standards | 09-dotnet10-native-aot-guidelines.md | — |
| .NET 10 Implementation | OBS-010 .NET 10 Implementation Guide | 08-opentelemetry-implementation.md | — |
| Reference Architecture | OBS-011 Observability Reference Architecture | 10-observability-reference-architecture.md | — |
| Cell Observability | OBS-012 Cell Observability Contract | 07-cell-observability-contract.md | — |
| Telemetry Storage | OBS-013 Telemetry Storage Strategy | 08-opentelemetry-implementation.md | — |
| Observability Security | OBS-014 Observability Security Model | 05-audit-ledger-specification.md | — |
| Governance | OBS-015 Observability Governance Model | OBS-CURRENT-STATE.md | — |
| Lifecycle | OBS-016 Observability Lifecycle Management | OBS-DOCUMENT-GENERATION-MANIFEST.md | — |
| Validation | OBS-017 Observability Validation Framework | OBS-DUPLICATE-RESOLUTION-REGISTER.md | — |
| Operational Runbook | OBS-018 Observability Operational Runbook | 00-observability-overview.md | — |

---

# 4. Reference Document Ownership

Reference documents support the following canonical documents.

| Reference Document | Supports |
|--------------------|----------|
| 00-observability-overview.md | OBS-000, OBS-008, OBS-018 |
| 01-observability-principles.md | OBS-001 |
| 02-logging-standard.md | OBS-003 |
| 03-distributed-tracing-standard.md | OBS-007 |
| 04-metrics-standard.md | OBS-004 |
| 05-audit-ledger-specification.md | OBS-005, OBS-014 |
| 06-correlation-context-specification.md | OBS-002, OBS-007 |
| 07-cell-observability-contract.md | OBS-012 |
| 08-opentelemetry-implementation.md | OBS-002, OBS-007, OBS-010, OBS-013 |
| 09-dotnet10-native-aot-guidelines.md | OBS-010 |
| 10-observability-reference-architecture.md | OBS-011 |
| 11-antigravity-agent-instructions.md | OBS-006 |

---

# 5. Governance Document Ownership

Governance documents control documentation generation and lifecycle.

| Governance Document | Responsibility |
|---------------------|----------------|
| OBS-CURRENT-STATE.md | Current project state |
| OBS-DOCUMENT-GENERATION-MANIFEST.md | Generation inventory |
| OBS-DOCUMENT-GENERATION-TRACEABILITY.md | Document lineage |
| OBS-DUPLICATE-RESOLUTION-REGISTER.md | Duplicate decisions |
| OBS-SCRIPT-GENERATION-STANDARDS.md | Script generation rules |
| OBS-CANONICAL-HIERARCHY.md | Documentation authority |
| OBS-DOCUMENT-MAPPING-MATRIX.md | Topic ownership |

---

# 6. Historical Documents

Historical documents are retained exclusively for documentation history.

They shall:

- never replace canonical architecture
- never override implementation guidance
- never be used as authoritative sources

Historical documents:

OBS-000-observability-architecture-index.md

OBS-001-observability-principles.md

OBS-002-logging-architecture.md

OBS-003-distributed-tracing-architecture.md

---

# 7. Authority Resolution Algorithm

When an architectural topic is requested:

Step 1

Locate the topic in this matrix.

Step 2

Read the canonical document.

Step 3

Read supporting documents if implementation detail is required.

Step 4

Ignore historical documents unless traceability is required.

---

# 8. Rules for Antigravity

Antigravity agents shall:

- identify the architecture topic
- locate the canonical owner
- consult supporting documentation only after reading the canonical document
- ignore historical artifacts during implementation
- preserve document authority during future updates

---

# 9. Maintenance Rules

Whenever a new Observability document is created:

- classify the document
- assign an owner
- update this matrix
- update the hierarchy document if authority changes
- update traceability if generation changes

---

# 10. Completion Status

Observability documentation organization:

COMPLETED

Canonical ownership:

ESTABLISHED

Document lineage:

VERIFIED

Duplicate resolution:

COMPLETED

Architecture authority:

DEFINED

---

End of Document