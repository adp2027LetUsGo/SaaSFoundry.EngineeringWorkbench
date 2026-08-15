# ============================================================================
# SaaSFoundry SDK
# Script: create-sdk-engineering-knowledge-part2.ps1
# Purpose: Generates Part 2 of the SDK Engineering Knowledge document.
# Version: 1.0
# ============================================================================

$ErrorActionPreference="Stop"

$ProjectRoot="C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"
$SdkRoot=Join-Path $ProjectRoot "docs\sdk"
$OutputFile=Join-Path $SdkRoot "09-SDK-ENGINEERING-KNOWLEDGE-v1.md"

$content=@"

# 5. Knowledge Lifecycle

Engineering knowledge SHALL progress through:

* Creation
* Review
* Approval
* Publication
* Maintenance
* Retirement

------------------------------------------------------------------------------

# 6. Governance

Engineering knowledge SHALL:

* Align with the SDK Canon
* Preserve traceability
* Avoid duplication
* Identify authoritative sources
* Be version controlled

------------------------------------------------------------------------------

# 7. Knowledge Consumers

* SDK Developers
* Plugin Developers
* Platform Architects
* AI Engineering Agents
* Technical Reviewers

------------------------------------------------------------------------------

# 8. Quality Attributes

Knowledge assets SHALL be:

* Accurate
* Consistent
* Discoverable
* Maintainable
* Reusable

------------------------------------------------------------------------------
"@

Add-Content -Path $OutputFile -Value $content -Encoding UTF8

Write-Host ""
Write-Host "Appended:"
Write-Host $OutputFile
