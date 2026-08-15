# ============================================================================
# SaaSFoundry SDK
# Script: create-sdk-adr-index-part2.ps1
# Purpose: Generates Part 2 of the SDK ADR Index document.
# Version: 1.0
# ============================================================================

$ErrorActionPreference="Stop"

$ProjectRoot="C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"
$SdkRoot=Join-Path $ProjectRoot "docs\sdk"
$OutputFile=Join-Path $SdkRoot "08-SDK-ADR-INDEX-v1.md"

$content=@"

# 5. ADR Lifecycle

Every ADR SHALL progress through:

* Proposed
* Approved
* Implemented
* Superseded
* Retired

------------------------------------------------------------------------------

# 6. Mandatory ADR Fields

Every ADR SHALL include:

* Identifier
* Title
* Status
* Context
* Decision
* Consequences
* References

------------------------------------------------------------------------------

# 7. Governance

The SDK Architecture Authority SHALL:

* Review ADRs
* Approve architectural changes
* Preserve Canon consistency
* Maintain traceability

------------------------------------------------------------------------------

# 8. Relationships

Each ADR SHOULD reference:

* SDK Handoff
* SDK Vision
* SDK Architecture
* EngineeringWorkbench System Canon

------------------------------------------------------------------------------
"@

Add-Content -Path $OutputFile -Value $content -Encoding UTF8

Write-Host ""
Write-Host "Appended:"
Write-Host $OutputFile
