# ============================================================================
# SaaSFoundry SDK
# Script: create-sdk-roadmap-part1.ps1
# Purpose: Generates Part 1 of the SDK Roadmap document.
# Version: 1.0
# ============================================================================

$ErrorActionPreference="Stop"

$ProjectRoot="C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"
$SdkRoot=Join-Path $ProjectRoot "docs\sdk"
$OutputFile=Join-Path $SdkRoot "06-SDK-ROADMAP-v1.md"

New-Item -ItemType Directory -Force -Path $SdkRoot | Out-Null

$content=@"
# SaaSFoundry SDK

# SDK ROADMAP

------------------------------------------------------------------------------
Document:
06-SDK-ROADMAP

Version:
1.0

Status:
ACTIVE

Classification:
OPERATIONAL

Authority:
SDK CURRENT STATE

Owner:
SaaSFoundry Engineering

------------------------------------------------------------------------------

# 1. Purpose

Defines the implementation roadmap for the SaaSFoundry SDK.

------------------------------------------------------------------------------

# 2. Vision

Deliver the official SDK for building EngineeringWorkbench plugins.

------------------------------------------------------------------------------

# 3. Phase 1

Foundation

Deliverables:

* SDK Canon
* SDK Solution
* SDK.Core
* SDK.Plugins

------------------------------------------------------------------------------

# 4. Phase 2

Developer Experience

Deliverables:

* Templates
* Validation Helpers
* Packaging Helpers
* Sample Plugins

------------------------------------------------------------------------------
"@

Set-Content -Path $OutputFile -Value $content -Encoding UTF8

Write-Host "Generated:"
Write-Host $OutputFile
