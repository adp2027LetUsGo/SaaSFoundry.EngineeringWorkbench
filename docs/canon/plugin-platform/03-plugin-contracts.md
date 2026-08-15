# Plugin Contracts

Version: 1.0

Status: Frozen

---

# Purpose

This document defines the mandatory contracts implemented by every Engineering Capability Plugin.

Plugins SHALL communicate only through explicit contracts.

---

# Design Principles

- Explicit
- Deterministic
- Stateless
- Versioned
- Native AOT Compatible

---

# Required Contracts

Every plugin SHALL implement:

- IEngineeringPlugin
- IPluginManifest
- IPluginCapability
- IPluginValidator
- IPluginExecutor
- IArtifactGenerator

---

# IEngineeringPlugin

Responsibilities:

- Identity
- Initialization
- Execution
- Shutdown

One implementation per plugin.

---

# IPluginManifest

Responsibilities:

- Plugin metadata
- Version
- Dependencies
- Supported capabilities
- Compatibility

Immutable.

---

# IPluginCapability

Responsibilities:

- Capability identifier
- Capability description
- Supported operations

One capability per implementation.

---

# IPluginValidator

Responsibilities:

- Validate configuration
- Validate input
- Validate output
- Produce validation evidence

Validation SHALL be deterministic.

---

# IPluginExecutor

Responsibilities:

- Receive execution context
- Execute capability
- Return execution result

Execution SHALL be stateless.

---

# IArtifactGenerator

Responsibilities:

- Generate engineering artifacts
- Report generated files
- Preserve traceability

Artifacts SHALL be immutable after generation.

---

# Versioning Rules

Contracts SHALL be versioned.

Breaking changes require a new major version.

---

# Dependency Rules

Contracts SHALL depend only on:

- Core abstractions

Contracts SHALL NOT depend on:

- Infrastructure
- UI
- CLI
- Other plugins

---

# Compatibility

Contracts SHALL support:

- .NET 10
- C# 14
- Native AOT

---

# Validation Rules

Every contract SHALL be:

- Testable
- Deterministic
- Explicit
- Minimal

---

# Acceptance Criteria

Plugin contracts are accepted when:

- Every required contract exists.
- Dependencies are explicit.
- No hidden coupling exists.
- Native AOT compatibility is preserved.
- Contract implementations are deterministic.

---

End of Plugin Contracts.
