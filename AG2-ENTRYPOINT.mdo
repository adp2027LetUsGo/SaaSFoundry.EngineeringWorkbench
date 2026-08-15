# AG2 ENTRYPOINT

Version: 1.0

Purpose:

This file is the operational entry point for Antigravity 2.0 implementation.

It does not define architecture.

The frozen architecture authority is the AG2-Package.

---

# Workspace

Target System:

SaaSFoundry Engineering Workbench

Implementation Root:

src/

Testing Root:

tests/

---

# Execution Instructions

Antigravity 2.0 SHALL:

1. Read the complete AG2-Package.
2. Respect the System Canon as immutable.
3. Use Plugin Platform specifications as implementation contracts.
4. Use Observability as the first reference plugin.
5. Generate the implementation under src/.
6. Generate validation tests under tests/.
7. Validate generated artifacts against the Canon.

---

# Reading Order

Read in this order:

1. docs/01-System-Canon

   SaaSFoundry.EngineeringWorkbench.SystemCanon.v1.md


2. docs/02-Plugin-Platform

   Plugin architecture and contracts


3. docs/03-Reference-Plugin/Observability

   Reference capability implementation


4. docs/04-AG2

   AG2 implementation instructions


---

# Implementation Constraints

Mandatory:

- .NET 10
- C# 14
- Native AOT compatible
- Clean Architecture
- Explicit plugin contracts
- No reflection-based architecture
- Linux ARM64 compatibility


---

# Existing Component

The existing Builder component:

SaaSFoundry.EngineeringWorkbench.Builder

is an internal Workbench module.

It SHALL be integrated, not replaced.

---

# Final Rule

The System Canon is the source of truth.

Do not redesign the architecture.

Implement the defined platform.

---

End of AG2 ENTRYPOINT.
