# ============================================================================
# SaaSFoundry SDK
# Script: create-sdk-engineering-knowledge-part1.ps1
# Purpose: Generates Part 1 of the SDK Engineering Knowledge document.
# Version: 1.0
# ============================================================================

$ErrorActionPreference="Stop"

$ProjectRoot="C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"
$SdkRoot=Join-Path $ProjectRoot "docs\sdk"
$OutputFile=Join-Path $SdkRoot "09-SDK-ENGINEERING-KNOWLEDGE-v1.md"

New-Item -ItemType Directory -Force -Path $SdkRoot | Out-Null

$content=@"
# SaaSFoundry SDK

# SDK ENGINEERING KNOWLEDGE

------------------------------------------------------------------------------
Document:
09-SDK-ENGINEERING-KNOWLEDGE

Version:
1.0

Status:
ACTIVE

Classification:
REFERENCE

Authority:
SDK ADR INDEX

Owner:
SaaSFoundry Engineering

------------------------------------------------------------------------------

# 1. Purpose

Defines the engineering knowledge assets supporting the SaaSFoundry SDK.

------------------------------------------------------------------------------

# 2. Knowledge Sources

* SDK Canon
* EngineeringWorkbench Canon
* ADRs
* Public Contracts
* Reference Plugins
* SDK Samples

------------------------------------------------------------------------------

# 3. Knowledge Principles

Knowledge SHALL be:

* Versioned
* Traceable
* Reviewable
* Reusable
* Authoritative

------------------------------------------------------------------------------

# 4. Knowledge Categories

* Constitutional
* Architectural
* Operational
* Reference

------------------------------------------------------------------------------
"@

Set-Content -Path $OutputFile -Value $content -Encoding UTF8

Write-Host "Generated:"
Write-Host $OutputFile
