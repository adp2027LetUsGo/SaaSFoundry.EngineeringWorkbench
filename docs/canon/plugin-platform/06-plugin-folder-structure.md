# Plugin Folder Structure

Version: 1.0

Status: Frozen

---

# 1. Purpose

This document defines the canonical folder structure for every Engineering Capability Plugin.

Every plugin SHALL follow this structure.

---

# 2. Canonical Structure

PluginName/

├── manifest/

├── contracts/

├── application/

├── domain/

├── infrastructure/

├── generators/

├── validators/

├── packaging/

├── resources/

├── templates/

├── tests/

└── README.md

---

# 3. Manifest

Contains:

- Plugin Manifest
- Capability metadata
- Version information
- Compatibility information

Exactly one manifest SHALL exist.

---

# 4. Contracts

Contains:

- Interfaces
- DTOs
- Public contracts

Contracts SHALL be independent.

---

# 5. Application

Contains:

- Use cases
- Orchestration
- Execution services

Business workflows belong here.

---

# 6. Domain

Contains:

- Domain entities
- Value objects
- Business rules

No infrastructure dependencies are permitted.

---

# 7. Infrastructure

Contains:

- External integrations
- Persistence
- File system access
- Serialization

Infrastructure SHALL remain isolated.

---

# 8. Generators

Contains:

- Artifact generators
- Template processors
- Output writers

Generation SHALL be deterministic.

---

# 9. Validators

Contains:

- Input validators
- Output validators
- Canon validators
- Acceptance validators

Validation SHALL produce engineering evidence.

---

# 10. Packaging

Contains:

- Packaging services
- Export providers
- Distribution builders

Packaging SHALL preserve traceability.

---

# 11. Resources

Contains:

- Static assets
- Embedded resources
- Localization files

Resources SHALL be immutable.

---

# 12. Templates

Contains:

- Markdown templates
- Source templates
- Configuration templates

Templates SHALL be version controlled.

---

# 13. Tests

Contains:

- Unit tests
- Integration tests
- Validation tests
- Acceptance tests

Every plugin SHALL include automated tests.

---

# 14. Naming Conventions

Folder names SHALL use:

- lowercase
- singular nouns
- deterministic naming

Directory structure SHALL remain consistent across all plugins.

---

# 15. Prohibited Directories

Plugins SHALL NOT create arbitrary directories.

Temporary files SHALL NOT be committed.

Build artifacts SHALL NOT be stored inside the plugin.

---

# 16. Extensibility

Future folders SHALL preserve backward compatibility.

Existing canonical folders SHALL NOT change meaning.

---

# 17. Native AOT Compatibility

Folder organization SHALL support:

- Compile-time registration
- Source generation
- Explicit contracts

No runtime discovery based on directory scanning SHALL be required.

---

# 18. Acceptance Criteria

The folder structure is compliant when:

- Canonical directories exist.
- Responsibilities are respected.
- Contracts remain isolated.
- Tests are included.
- Native AOT compatibility is preserved.

---

End of Plugin Folder Structure.
