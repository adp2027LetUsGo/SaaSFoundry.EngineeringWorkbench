# ============================================================================
# SaaSFoundry SDK
# Script: create-sdk-architecture-freeze-part1.ps1
# Purpose: Generates Part 1 of the SDK Architecture Freeze document.
# Version: 1.0
# ============================================================================

$ErrorActionPreference = "Stop"

$ProjectRoot = "C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"
$SdkRoot     = Join-Path $ProjectRoot "docs\sdk"
$OutputFile  = Join-Path $SdkRoot "03-SDK-ARCHITECTURE-FREEZE-v1.md"

New-Item -ItemType Directory -Force -Path $SdkRoot | Out-Null

$content = @"
# SaaSFoundry SDK

# SDK ARCHITECTURE FREEZE

------------------------------------------------------------------------------
Document:
03-SDK-ARCHITECTURE-FREEZE

Version:
1.0

Status:
FROZEN

Classification:
CONSTITUTIONAL

Authority:
SDK ARCHITECTURE

Owner:
SaaSFoundry Engineering

------------------------------------------------------------------------------

# 1. Purpose

Defines the architectural elements of the SDK that are frozen and require
architectural approval before modification.

------------------------------------------------------------------------------

# 2. Frozen Components

The following SHALL remain stable:

* Public SDK Contracts
* Package Boundaries
* Public Namespaces
* SDK Extension Model
* Compatibility Model

------------------------------------------------------------------------------

# 3. Frozen Principles

* Native AOT First
* Contract First
* Canon First
* Backward Compatibility
* Deterministic Behavior

------------------------------------------------------------------------------

# 4. Architectural Boundaries

The SDK SHALL NOT expose EngineeringWorkbench internal implementation
details.

------------------------------------------------------------------------------
"@

Set-Content -Path $OutputFile -Value $content -Encoding UTF8

Write-Host "Generated:"
Write-Host $OutputFile
