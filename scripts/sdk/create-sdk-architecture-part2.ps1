# ============================================================================
# SaaSFoundry SDK
# Script: create-sdk-architecture-part2.ps1
# Purpose: Generates Part 2 of the SDK Architecture document.
# Version: 1.0
# ============================================================================

$ErrorActionPreference = "Stop"

$ProjectRoot = "C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"
$SdkRoot     = Join-Path $ProjectRoot "docs\sdk"
$OutputFile  = Join-Path $SdkRoot "02-SDK-ARCHITECTURE-v1.md"

$content = @"

# 5. Architectural Layers

The SDK SHALL be organized into:

* Core
* Plugin APIs
* Validation APIs
* Packaging APIs
* Testing APIs

------------------------------------------------------------------------------

# 6. Component Responsibilities

SDK.Core
    Shared contracts and primitives.

SDK.Plugins
    Plugin authoring APIs.

SDK.Validation
    Validation helpers.

SDK.Packaging
    Packaging helpers.

SDK.Testing
    Test infrastructure for plugins.

------------------------------------------------------------------------------

# 7. Dependency Rules

* Core SHALL have no SDK dependencies.
* Higher layers MAY depend on lower layers.
* Circular dependencies are prohibited.
* Runtime assemblies SHALL NOT depend on the SDK.

------------------------------------------------------------------------------

# 8. Integration Model

The SDK SHALL communicate with the EngineeringWorkbench exclusively
through published public contracts.

No internal runtime implementation details SHALL be exposed.

------------------------------------------------------------------------------
"@

Add-Content -Path $OutputFile -Value $content -Encoding UTF8

Write-Host ""
Write-Host "Appended:"
Write-Host $OutputFile
