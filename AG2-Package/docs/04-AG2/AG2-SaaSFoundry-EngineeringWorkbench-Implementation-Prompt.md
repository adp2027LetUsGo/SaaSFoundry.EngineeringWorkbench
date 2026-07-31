# AG2 SaaSFoundry Engineering Workbench Implementation Prompt

Version: 1.0

Status: Execution Specification

---

# Mission

Implement SaaSFoundry Engineering Workbench according to the frozen System Canon.

The system is a plugin-based architecture execution platform.

The existing Builder component must become an internal module of the Workbench.

Do not redesign the architecture.

Do not introduce alternative patterns.

Implement according to the provided engineering package.

---

# Source of Truth

The implementation SHALL follow:

1. SaaSFoundry Engineering Workbench System Canon
2. Plugin Platform Specification
3. Observability Reference Plugin Canon
4. Observability Implementation Library

These documents are authoritative.

---

# Target Architecture

Implement:

SaaSFoundry.EngineeringWorkbench

with the following components:

- SaaSFoundry.EngineeringWorkbench.Core
- SaaSFoundry.EngineeringWorkbench.Application
- SaaSFoundry.EngineeringWorkbench.Infrastructure
- SaaSFoundry.EngineeringWorkbench.PluginRuntime
- SaaSFoundry.EngineeringWorkbench.Builder
- SaaSFoundry.EngineeringWorkbench.Validation
- SaaSFoundry.EngineeringWorkbench.Packaging
- SaaSFoundry.EngineeringWorkbench.Cli
- SaaSFoundry.EngineeringWorkbench.UI

---

# Architectural Principles

The implementation SHALL follow:

- Clean Architecture
- Explicit contracts
- Plugin-based extensibility
- Deterministic execution
- Canon-first engineering
- Evidence-driven validation

---

# Existing Builder Integration

The existing:

SaaSFoundry.EngineeringWorkbench.Builder

is not a separate product.

It SHALL become an internal Workbench capability.

Preserve:

- artifact generation
- template processing
- package generation

Integrate it into the Workbench execution flow.

---

# First Reference Plugin

Implement Observability as the first Engineering Capability Plugin.

Use:

OBS-000 through OBS-099

as the architectural definition.

Use:

OBS-100 through OBS-199

as the implementation reference library.

---

# Technology Constraints

Mandatory:

- .NET 10
- C# 14
- Native AOT compatible
- Clean Architecture
- Windows development environment
- Linux ARM64 deployment compatibility

---

# Native AOT Requirements

The implementation SHALL NOT depend on:

- runtime reflection
- assembly scanning
- dynamic proxy generation
- runtime code generation

Plugin registration SHALL be explicit.

---

# Plugin Requirements

Plugins SHALL:

- implement explicit contracts
- expose declared capabilities
- follow lifecycle rules
- validate execution
- generate traceable artifacts

---

# Validation Requirements

Every execution SHALL produce:

- validation results
- engineering evidence
- traceability information

---

# CLI Requirements

Provide command-line execution for:

- capability execution
- validation
- artifact generation
- packaging

---

# Definition of Done

The implementation is complete when:

- Workbench solution builds successfully.
- Plugin Runtime executes capabilities.
- Builder operates as an internal module.
- Observability plugin executes successfully.
- Generated artifacts are validated.
- Engineering evidence is produced.
- Native AOT compatibility requirements are satisfied.
- Implementation conforms to the System Canon.

---

# Final Instruction

Implement the SaaSFoundry Engineering Workbench.

Treat the supplied Canon and specifications as immutable architecture contracts.

Generate code only after understanding the complete engineering package.

---

End of AG2 Implementation Prompt.
