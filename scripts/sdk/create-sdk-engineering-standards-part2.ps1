# ============================================================================
# SaaSFoundry SDK
# Script: create-sdk-engineering-standards-part2.ps1
# Purpose: Generates Part 2 of the SDK Engineering Standards document.
# Version: 1.0
# ============================================================================

$ErrorActionPreference="Stop"

$ProjectRoot="C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"
$SdkRoot=Join-Path $ProjectRoot "docs\sdk"
$OutputFile=Join-Path $SdkRoot "04-SDK-ENGINEERING-STANDARDS-v1.md"

$content=@"

# 5. Public API Standards

Public APIs SHALL be:

* Stable
* Discoverable
* Strongly Typed
* Versioned
* Fully Documented

------------------------------------------------------------------------------

# 6. Dependency Standards

The SDK SHALL:

* Minimize external dependencies.
* Avoid circular references.
* Depend only on published runtime contracts.
* Preserve Native AOT compatibility.

------------------------------------------------------------------------------

# 7. Testing Standards

Every SDK component SHALL include:

* Unit Tests
* Integration Tests
* API Compatibility Validation
* Native AOT Verification

------------------------------------------------------------------------------

# 8. Documentation Standards

Every public component SHALL provide:

* XML Documentation
* Usage Examples
* Version Information
* Canon References

------------------------------------------------------------------------------
"@

Add-Content -Path $OutputFile -Value $content -Encoding UTF8

Write-Host ""
Write-Host "Appended:"
Write-Host $OutputFile
