# Antigravity 2.0 — Configuration & Operating Environment Guide

**Document:** AG2-OPERATING-ENVIRONMENT-GUIDE  
**Version:** 1.0  
**Status:** ACTIVE GUIDE

## 1. Purpose

Operational guide for configuring Antigravity 2.0 (AG2) to work with SaaSFoundry EngineeringWorkbench.

Scope:

1. Configuration / Operating Environment
2. Package / agents / context / execution
3. Relationship with the Execution Engine
4. Environment preparation

This guide is separate from the SaaSFoundry SDK Canon.

## 2. Core Operating Model

AG2 is treated as an external engineering execution environment / implementation agent.

The EngineeringWorkbench remains the architectural authority.

```text
SaaSFoundry System Canon
        |
        v
EngineeringWorkbench Architecture
        |
        v
EngineeringWorkbench Runtime / Planner / Governance
        |
        v
Antigravity 2.0 Operating Environment
        |
        +-- AG2 Package
        +-- AG2 Entry Point
        +-- .agents
        +-- RAG / Knowledge Context
        +-- Execution Instructions
        |
        v
Repository implementation
        +-- src/
        +-- tests/
```

AG2 does not become the architectural authority.

## 3. Architectural Authority

AG2 SHALL:

- Read the AG2 Package before implementation.
- Treat the System Canon as immutable/source of truth.
- Respect the frozen EngineeringWorkbench architecture.
- Use existing Plugin Platform contracts.
- Use Observability as the first production reference plugin.
- Implement within the existing solution.
- Generate production code under `src/`.
- Generate tests under `tests/`.
- Avoid redesigning the architecture.

AG2 SHALL NOT:

- Redesign the frozen architecture.
- Create a parallel architecture.
- Replace canonical contracts without authorization.
- Create duplicate projects when an existing project fulfills the role.
- Treat its own implementation preferences as architectural authority.

## 4. EngineeringWorkbench Repository

Established repository root:

```text
C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench
```

Existing structure includes:

```text
.sln
src/
tests/
AG2-Package/
AG2-ENTRYPOINT.md
```

The existing solution SHALL be evolved rather than creating a parallel solution.

```text
SaaSFoundry.EngineeringWorkbench.sln
```

## 5. AG2-Package

`AG2-Package` is the implementation context package prepared for Antigravity 2.0.

Its purpose is to provide the authoritative engineering material required for implementation without reconstructing the architecture from conversation history.

AG2 SHALL read the package before beginning implementation.

## 6. AG2-ENTRYPOINT.md

The repository contains:

```text
AG2-ENTRYPOINT.md
```

This is the operational landing page for AG2.

It is intentionally outside `docs/`.

It is not a new architectural layer.

Its purpose is to provide the operational entry point and direct AG2 toward authoritative implementation context.

## 7. .agents

The `.agents` area belongs to the AG2 operating environment.

It provides agent-oriented execution instructions and context.

It SHALL NOT replace:

- System Canon
- Architecture Freeze
- Runtime contracts
- Engineering governance

## 8. Context Loading Principle

Recommended conceptual loading order:

```text
1. AG2 Entry Point
        |
2. AG2 Package
        |
3. System Canon
        |
4. Architecture Freeze
        |
5. Plugin Platform Contracts
        |
6. Current implementation state
        |
7. Specific implementation task
```

The objective is to prevent implementation of a local task without understanding its architectural constraints.

## 9. RAG Knowledge Layer

A canonical RAG configuration was established to provide relevant knowledge to Antigravity 2.0 without bypassing Canon authority.

```yaml
rag:
  enabled: true
  mode: hybrid
  purpose: >
    Provide relevant knowledge to Antigravity 2.0 without
    bypassing Canon authority.
  corpus:
    roots:
      - docs/
      - foundation/
      - projects/
    exclude:
      - .git/
      - .agents/
      - node_modules/
      - bin/
      - obj/
      - dist/
      - build/
      - coverage/
      - archive/
    include_extensions:
      - .md
      - .mdx
      - .yaml
      - .yml
      - .json
  chunking:
    strategy: heading-aware
    max_tokens: 800
    overlap_tokens: 120
    preserve_metadata: true
    preserve_code_blocks: true
    preserve_tables: true
```

RAG provides knowledge retrieval. It does not override Canon authority.

The established configuration explicitly excludes `.agents/` from the RAG corpus.

## 10. Execution Environment

AG2 should operate from:

```text
C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench
```

This provides access to the solution, source, tests, AG2 Package, entry point, Canon, plugins and implementation state.

AG2 must not assume that the repository is empty.

## 11. Existing Solution Rule

Use the existing solution:

```text
SaaSFoundry.EngineeringWorkbench.sln
```

The existing Workbench must be evolved rather than paralleled.

If a capability already exists, integrate/evolve it instead of creating a duplicate implementation.

## 12. Source and Test Boundaries

Production code:

```text
src/
```

Tests:

```text
tests/
```

Production implementation must not be placed in documentation directories.

## 13. Plugin Development Context

The EngineeringWorkbench uses a plugin-oriented architecture.

Plugin Platform contracts are authoritative.

The first production reference plugin is:

```text
SaaSFoundry.Plugins.Observability
```

Observability is the reference implementation for understanding production plugin integration. It is not permission to redesign the Plugin Platform.

## 14. Relationship with the Execution Engine

Keep the Execution Engine conceptually separate from the AG2 operating environment.

```text
Execution Engine
    |
    v
EngineeringWorkbench
    |
    v
AG2 Operating Environment
    |
    v
Repository implementation
```

AG2 performs implementation work. It does not own EngineeringWorkbench architectural decisions.

## 15. Planner and Governance Context

AG2 implementation must remain compatible with:

- Engineering Planner
- Engineering Catalog
- Validation Engine
- Packaging Engine
- Execution Governance
- Plugin Runtime

The agent performs implementation while the platform remains responsible for architectural and execution governance.

## 16. Standard AG2 Implementation Workflow

### Step 1 — Enter repository

```text
C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench
```

### Step 2 — Read entry point

```text
AG2-ENTRYPOINT.md
```

### Step 3 — Load AG2 Package

Read the supplied implementation context.

### Step 4 — Load Canon

Read the relevant System Canon and Architecture Freeze.

### Step 5 — Inspect implementation

Inspect:

```text
src/
tests/
```

before creating projects or replacing existing implementations.

### Step 6 — Inspect plugin contracts

For plugin work, inspect the Plugin Platform contracts and reference implementation.

### Step 7 — Execute task

Implement only the requested scope.

### Step 8 — Validate

Run relevant:

- Build
- Unit tests
- Integration tests
- Contract validation
- Native AOT checks where applicable

### Step 9 — Report

Return:

- Changes
- Files created/modified
- Tests executed
- Build status
- Remaining issues

## 17. Safety Rules

AG2 must stop and reassess when a task appears to require:

- Changing a frozen contract
- Changing the Architecture Freeze
- Creating a parallel subsystem
- Duplicating an existing platform component
- Moving architectural boundaries
- Introducing an unauthorized architectural pattern

The correct behavior is to identify the conflict rather than silently redesign the system.

## 18. AG2 Package Design Principle

The package should make explicit:

```text
WHAT exists
WHY it exists
WHERE it exists
WHAT is frozen
WHAT may change
WHAT must not change
WHAT task is being executed
HOW success is verified
```

This prevents AG2 from depending on conversational memory.

## 19. Prompt / Handoff Pattern

An implementation prompt should normally contain:

```text
1. Mission
2. Current State
3. Relevant Canon
4. Frozen Boundaries
5. Exact Task
6. Files / Projects in Scope
7. Required Tests
8. Acceptance Criteria
9. Forbidden Changes
10. Expected Completion Report
```

Example:

```text
MISSION
Implement <specific capability>.

AUTHORITIES
Use the EngineeringWorkbench Canon and Architecture Freeze.

CURRENT STATE
<current implementation>

SCOPE
<projects/files>

REQUIRED
<implementation requirements>

FORBIDDEN
Do not redesign frozen architecture.
Do not create duplicate projects.

VALIDATION
Build and run relevant tests.

DELIVERABLE
Provide implementation summary and validation results.
```

## 20. Deterministic Execution Principle

Prefer deterministic instructions.

Avoid:

```text
Improve the architecture.
```

Prefer:

```text
Implement capability X in project Y.
Do not modify contracts A, B, or C.
Add tests under tests/Z.
Run the existing test suite.
```

## 21. Authority Hierarchy

```text
System Canon
    |
Architecture Freeze
    |
Platform Contracts
    |
Engineering Plan
    |
AG2 Task / Prompt
    |
Implementation
```

A lower-level instruction must not silently override a higher-level authority.

## 22. RAG Relationship

Responsibilities are separate:

```text
Canon
  = authority

RAG
  = knowledge retrieval

AG2
  = implementation execution
```

## 23. Preparing a New Machine

1. Restore the EngineeringWorkbench repository.
2. Verify the solution exists.
3. Verify `AG2-Package/` exists.
4. Verify `AG2-ENTRYPOINT.md` exists.
5. Verify `.agents/` exists where required by the AG2 environment.
6. Verify the Canon is available.
7. Verify Plugin Platform contracts are available.
8. Verify RAG corpus roots and exclusions.
9. Open AG2 in repository context.
10. Load the AG2 entry point.
11. Load the package.
12. Execute a small validation task before production implementation.

## 24. Environment Validation Checklist

```text
[ ] Repository root is correct
[ ] SaaSFoundry.EngineeringWorkbench.sln exists
[ ] src/ exists
[ ] tests/ exists
[ ] AG2-Package/ exists
[ ] AG2-ENTRYPOINT.md exists
[ ] Canon is available
[ ] Architecture Freeze is available
[ ] Plugin Platform contracts are available
[ ] Observability reference plugin is available
[ ] .agents configuration is available
[ ] RAG configuration is available
[ ] RAG excludes .agents/
[ ] AG2 can read required context
[ ] AG2 understands frozen boundaries
[ ] Build works
[ ] Tests work
```

## 25. Common Failure Modes

### AG2 redesigns architecture

Cause: insufficient Canon context or ambiguous prompt.

Correction: provide Architecture Freeze and explicit forbidden changes.

### AG2 creates duplicate projects

Cause: the agent did not inspect the existing solution.

Correction: require repository inspection before implementation.

### AG2 modifies frozen contracts

Cause: task scope was not constrained.

Correction: explicitly identify immutable contracts.

### AG2 relies on conversation history

Cause: insufficient AG2 Package.

Correction: move authoritative context into versioned repository artifacts.

### RAG returns irrelevant knowledge

Cause: incorrect corpus configuration or retrieval.

Correction: verify roots, exclusions, extensions and heading-aware chunking.

## 26. Operational Golden Rules

1. **Canon is authority.**
2. **AG2 implements; it does not redefine architecture.**
3. **Inspect before creating.**
4. **Use existing contracts.**
5. **Use the existing solution.**
6. **Keep production code in `src/`.**
7. **Keep tests in `tests/`.**
8. **Use Observability as the reference plugin.**
9. **Use RAG for retrieval, not authority.**
10. **Make implementation tasks deterministic.**

## 27. Relationship to the SDK

The SDK is a separate architectural workstream.

```text
EngineeringWorkbench
        |
        +-- Plugin Runtime
        +-- Plugin Platform
        |
        +-- SDK
              |
              v
        Plugin Development
```

Antigravity 2.0 is the external implementation environment used to build these components.

Therefore:

```text
Antigravity 2.0 != SDK
Antigravity 2.0 != EngineeringWorkbench
```

## 28. Operational Separation

Keep these workstreams separate:

```text
WORKSTREAM A
Antigravity 2.0
Operating Environment
Agents
Context
RAG
Execution

WORKSTREAM B
EngineeringWorkbench
System Canon
Runtime
Planner
Governance
Plugins

WORKSTREAM C
SDK
SDK Canon
SDK APIs
SDK Packages
SDK Tooling
```

They interact, but they are not the same system.

## 29. Established Baseline

```text
EngineeringWorkbench:
SaaSFoundry.EngineeringWorkbench

Repository:
C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench

Solution:
SaaSFoundry.EngineeringWorkbench.sln

Operational Entry Point:
AG2-ENTRYPOINT.md

Agent Context:
AG2-Package/

Agent Environment:
.agents/

Reference Plugin:
SaaSFoundry.Plugins.Observability
```

## 30. Scope Note

This guide records the Antigravity 2.0 operating principles established in the available SaaSFoundry project context.

It does not claim undocumented product-specific AG2 UI settings, authentication settings, vendor configuration fields, or other details that are not present in the established project context.

Such details should only be added after verification from the actual AG2 environment or authoritative AG2 documentation.

## 31. Final Operational Model

```text
                    SYSTEM CANON
                         |
                         v
              ENGINEERINGWORKBENCH
                         |
          +--------------+--------------+
          v              v              v
       Planner        Runtime       Governance
                         |
                         v
                  Plugin Platform
                         |
          +--------------+--------------+
          v                             v
        Plugins                         SDK
          |                             |
          +--------------+--------------+
                         |
                         v
               AG2 IMPLEMENTATION
                         |
                 +-------+-------+
                 v               v
               src/            tests/
```

Antigravity 2.0 is the implementation environment.

EngineeringWorkbench is the platform.

The Canon is the authority.

RAG is the knowledge retrieval mechanism.

The SDK is the developer-facing framework.

Plugins are the extensibility mechanism.

## 32. Document Maintenance

Update this guide whenever the AG2 operating environment changes materially, including:

- AG2 Package structure changes
- `.agents` conventions
- RAG configuration changes
- Entry Point changes
- Execution workflow changes
- New agent roles
- Context loading changes
- Execution Engine integration changes

# END OF DOCUMENT
