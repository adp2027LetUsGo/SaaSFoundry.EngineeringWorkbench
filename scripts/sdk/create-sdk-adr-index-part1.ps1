# ============================================================================
# SaaSFoundry SDK
# Script: create-sdk-adr-index-part1.ps1
# Purpose: Generates Part 1 of the SDK ADR Index document.
# Version: 1.0
# ============================================================================

$ErrorActionPreference="Stop"

$ProjectRoot="C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"
$SdkRoot=Join-Path $ProjectRoot "docs\sdk"
$OutputFile=Join-Path $SdkRoot "08-SDK-ADR-INDEX-v1.md"

New-Item -ItemType Directory -Force -Path $SdkRoot | Out-Null

$content=@"
# SaaSFoundry SDK

# SDK ARCHITECTURAL DECISION RECORD INDEX

------------------------------------------------------------------------------
Document:
08-SDK-ADR-INDEX

Version:
1.0

Status:
ACTIVE

Classification:
REFERENCE

Authority:
SDK ARCHITECTURE

Owner:
SaaSFoundry Engineering

------------------------------------------------------------------------------

# 1. Purpose

Defines the Architectural Decision Record (ADR) index for the SDK.

------------------------------------------------------------------------------

# 2. ADR Policy

Every significant SDK architectural decision SHALL be documented.

------------------------------------------------------------------------------

# 3. Initial ADR Register

ADR-001 SDK Foundation

ADR-002 Public API Design

ADR-003 Native AOT First

ADR-004 Package Boundaries

------------------------------------------------------------------------------

# 4. ADR Categories

* Architecture
* APIs
* Packaging
* Compatibility
* Tooling

------------------------------------------------------------------------------
"@

Set-Content -Path $OutputFile -Value $content -Encoding UTF8

Write-Host "Generated:"
Write-Host $OutputFile
