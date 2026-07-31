# OBS-SCRIPT-GENERATION-STANDARDS.md

# SaaS-Foundry Observability Script Generation Standards

Document ID:
OBS-GEN-STANDARDS-001

Version:
1.1

Status:
Canon

Scope:
SaaS-Foundry Observability Documentation Automation

Audience:
- Platform Architects
- Documentation Engineers
- Antigravity Agents
- Automation Maintainers

---

# 1. Purpose

This document defines the mandatory standards for PowerShell scripts
used during SaaS-Foundry Observability documentation generation.

The objective is to guarantee:

- deterministic execution
- repeatable generation
- environment independence
- documentation consistency
- safe continuation across AI sessions

---

# 2. Script Location

Canonical location:

C:\Users\armando\Documents\_AHS\projects\SaaS-Foundry\scripts\observability

Scripts must be executable from any directory.

Scripts must not depend on the current working directory.

---

# 3. Path Standards

## Required

All scripts must use absolute paths.

Example:

C:\Users\armando\Documents\_AHS\projects\SaaS-Foundry\docs\architecture\observability


## Forbidden

Do not use:

$PSScriptRoot

Split-Path

Relative paths

.\

..\


Reason:

The generation environment must be deterministic.

---

# 4. Encoding Standards

## PowerShell Scripts

All .ps1 files must contain ASCII characters only.

Avoid:

- Unicode punctuation
- Smart quotes
- Unicode arrows
- Special symbols
- Invisible formatting characters


Allowed:

Standard ASCII characters.

---

## Generated Markdown

All generated Markdown files must use UTF-8 encoding.

Required:

Set-Content -Encoding UTF8


Markdown content may contain normal documentation characters.

---

# 5. Script Naming Convention

Standard:

create-observability-docs-part<N>.ps1


Examples:

create-observability-docs-part1.ps1

create-observability-docs-part1a.ps1

create-observability-docs-part1b.ps1

create-observability-docs-part2.ps1

create-observability-docs-part3.ps1


---

# 6. Script Execution Model

Scripts are executed in controlled sequence.

Current known sequence:

1.

create-observability-docs-part1.ps1


2.

create-observability-docs-part1a.ps1


3.

create-observability-docs-part1b.ps1


4.

create-observability-docs-part2.ps1


5.

create-observability-docs-part3.ps1


---

# 7. Idempotency Rules

Scripts must be safe for repeated execution.

Repeated execution must:

- not duplicate content
- not corrupt documents
- not create unnecessary files
- preserve existing architecture


---

# 8. Document Protection Rules

Existing documents must not be modified unless explicitly targeted.

Examples:

A script generating OBS-004 must not modify OBS-000.

A consolidation task must not silently rewrite canonical documents.

---

# 9. Generation Scope Rules

Scripts are responsible only for:

- creating requested documents
- updating explicitly targeted documents
- maintaining deterministic output


Scripts must not:

- redefine architecture
- introduce unrelated technologies
- change naming conventions
- create duplicate document families without approval


---

# 10. Current Script State

Available:

[X] create-observability-docs-part1.ps1

[X] create-observability-docs-part1a.ps1

[X] create-observability-docs-part1b.ps1

[X] create-observability-docs-part2.ps1

[X] create-observability-docs-part3.ps1


Current status:

Generation scripts exist.

Additional generation is paused.

Current phase:

Documentation consolidation and canonization.

---

# 11. Future Script Requirements

Any future observability generation script must:

- follow this standard
- update OBS-CURRENT-STATE.md
- preserve OBS-DOCUMENT-GENERATION-MANIFEST.md
- use absolute paths
- generate UTF-8 Markdown


---

# 12. Validation Checklist

Before execution:

[ ] Absolute paths verified.

[ ] Script contains ASCII only.

[ ] Output directory verified.

[ ] Document targets verified.

[ ] Existing documents protected.

[ ] UTF-8 output configured.

[ ] Current state updated after execution.


---

# 13. AI Continuation Rule

When continuing from a new AI session:

The AI must read:

OBS-DOCUMENT-GENERATION-MANIFEST.md

OBS-SCRIPT-GENERATION-STANDARDS.md

OBS-CURRENT-STATE.md


The AI must determine the current state before generating new artifacts.

---

End of Document