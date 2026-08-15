# ============================================================================
# SaaSFoundry SDK
# Script: create-sdk-architecture-part1.ps1
# Purpose: Generates Part 1 of the SDK Architecture document.
# Version: 1.0
# ============================================================================

$ErrorActionPreference = "Stop"

$ProjectRoot = "C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"
$SdkRoot     = Join-Path $ProjectRoot "docs\sdk"
$OutputFile  = Join-Path $SdkRoot "02-SDK-ARCHITECTURE-v1.md"

New-Item -ItemType Directory -Force -Path $SdkRoot | Out-Null

$content = @"
# SaaSFoundry SDK

# SDK ARCHITECTURE

------------------------------------------------------------------------------
Document:
02-SDK-ARCHITECTURE

Version:
1.0

Status:
DRAFT

Classification:
CONSTITUTIONAL

Authority:
SDK HANDOFF

Owner:
SaaSFoundry Engineering

------------------------------------------------------------------------------

# 1. Purpose

Defines the canonical architecture of the SaaSFoundry SDK.

------------------------------------------------------------------------------

# 2. Architectural Goals

* Native AOT First
* Strongly Typed APIs
* Stable Public Contracts
* Minimal Dependencies
* Plugin Developer Productivity

------------------------------------------------------------------------------

# 3. Proposed Solution

src/

* SaaSFoundry.SDK.Core
* SaaSFoundry.SDK.Plugins
* SaaSFoundry.SDK.Validation
* SaaSFoundry.SDK.Packaging
* SaaSFoundry.SDK.Testing

------------------------------------------------------------------------------

# 4. Design Principles

* Canon First
* Contract First
* Explicit Dependencies
* Deterministic Behavior
* Backward Compatibility

------------------------------------------------------------------------------
"@

Set-Content -Path $OutputFile -Value $content -Encoding UTF8

Write-Host ""
Write-Host "Generated:"
Write-Host $OutputFile
