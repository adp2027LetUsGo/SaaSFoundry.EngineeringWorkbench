# Plugin Manifest Specification

Version: 1.0

Status: Frozen

---

# 1. Purpose

The Plugin Manifest defines the immutable metadata describing an Engineering Capability Plugin.

The manifest is the primary discovery mechanism used by the Plugin Runtime.

Every plugin SHALL provide exactly one manifest.

---

# 2. Design Principles

The manifest SHALL be:

- Explicit
- Immutable
- Versioned
- Deterministic
- Human readable
- Machine readable
- Native AOT compatible

---

# 3. Responsibilities

The manifest defines:

- Plugin identity
- Version
- Capability
- Compatibility
- Dependencies
- Execution metadata

The manifest SHALL NOT contain implementation logic.

---

# 4. Required Metadata

Every manifest SHALL define:

- PluginId
- Name
- DisplayName
- Description
- Version
- Author
- Company
- Website
- License
- Capability
- Category
- Tags

---

# 5. Runtime Metadata

Every manifest SHALL define:

- MinimumWorkbenchVersion
- MaximumWorkbenchVersion
- MinimumDotNetVersion
- NativeAOTSupported
- SupportedOperatingSystems

---

# 6. Dependency Metadata

Every manifest SHALL declare:

- Required plugins
- Optional plugins
- External packages
- Runtime dependencies

Dependencies SHALL be explicit.

---

# 7. Capability Metadata

The manifest SHALL identify:

- Primary capability
- Supported operations
- Generated artifact types
- Validation support
- Packaging support

---

# 8. Execution Metadata

The manifest SHALL specify:

- Execution mode
- Execution order
- Priority
- Parallel execution support
- Cancellation support

---

# 9. Validation Metadata

The manifest SHALL indicate:

- Validation required
- Validation strategy
- Evidence generation
- Acceptance verification

---

# 10. Versioning

Manifest versions SHALL follow Semantic Versioning.

Breaking changes require a major version increment.

---

# 11. Compatibility

The manifest SHALL support:

- .NET 10
- C# 14
- Native AOT

---

# 12. Security

The manifest SHALL NOT:

- Execute code
- Load assemblies
- Modify configuration

The manifest is metadata only.

---

# 13. Acceptance Criteria

A manifest is compliant when:

- All mandatory metadata exists.
- Metadata is immutable.
- Dependencies are explicit.
- Version information is complete.
- Runtime compatibility is declared.

---

End of Plugin Manifest Specification.
