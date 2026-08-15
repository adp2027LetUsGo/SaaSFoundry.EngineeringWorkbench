# ============================================================================
# SaaSFoundry SDK
# Script: create-sdk-backlog-part1.ps1
# Purpose: Generates Part 1 of the SDK Backlog document.
# Version: 1.0
# ============================================================================

$ErrorActionPreference="Stop"

$ProjectRoot="C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"
$SdkRoot=Join-Path $ProjectRoot "docs\sdk"
$OutputFile=Join-Path $SdkRoot "07-SDK-BACKLOG-v1.md"

New-Item -ItemType Directory -Force -Path $SdkRoot | Out-Null

$content=@"
# SaaSFoundry SDK

# SDK BACKLOG

------------------------------------------------------------------------------
Document:
07-SDK-BACKLOG

Version:
1.0

Status:
ACTIVE

Classification:
OPERATIONAL

Authority:
SDK ROADMAP

Owner:
SaaSFoundry Engineering

------------------------------------------------------------------------------

# 1. Purpose

Defines the prioritized engineering backlog for the SaaSFoundry SDK.

------------------------------------------------------------------------------

# 2. High Priority

* Create SDK solution
* Implement SDK.Core
* Implement SDK.Plugins
* Define public APIs
* Publish first samples

------------------------------------------------------------------------------

# 3. Medium Priority

* Validation helpers
* Packaging helpers
* Testing framework
* Documentation improvements

------------------------------------------------------------------------------

# 4. Low Priority

* Visual Studio tooling
* Roslyn analyzers
* Source generators
* Additional templates

------------------------------------------------------------------------------
"@

Set-Content -Path $OutputFile -Value $content -Encoding UTF8

Write-Host "Generated:"
Write-Host $OutputFile
