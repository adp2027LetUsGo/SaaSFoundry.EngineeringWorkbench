# ============================================================================
# SaaSFoundry SDK
# Script: create-sdk-adr-index-part3.ps1
# Purpose: Generates Part 3 of the SDK ADR Index document.
# Version: 1.0
# ============================================================================

$ErrorActionPreference="Stop"

$ProjectRoot="C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"
$SdkRoot=Join-Path $ProjectRoot "docs\sdk"
$OutputFile=Join-Path $SdkRoot "08-SDK-ADR-INDEX-v1.md"

$content=@"

# 9. Review Process

Every ADR SHALL be reviewed before implementation to verify:

* Canon compliance
* Architectural consistency
* Technical feasibility
* Long-term maintainability

------------------------------------------------------------------------------

# 10. Numbering

ADR identifiers SHALL be sequential and SHALL NOT be reused.

Example:

ADR-001
ADR-002
ADR-003

------------------------------------------------------------------------------

# 11. Maintenance

The ADR Index SHALL be updated whenever:

* A new ADR is approved
* An ADR is superseded
* An ADR is retired

------------------------------------------------------------------------------

# 12. Revision History

Version 1.0

Initial SDK ADR Index specification.

------------------------------------------------------------------------------
END OF DOCUMENT
"@

Add-Content -Path $OutputFile -Value $content -Encoding UTF8

Write-Host ""
Write-Host "Appended:"
Write-Host $OutputFile
