# ============================================================================
# SaaSFoundry SDK
# Script: create-sdk-engineering-standards.ps1
# Purpose: Orchestrates complete generation of the SDK Engineering Standards.
# Version: 1.0
# ============================================================================

$ErrorActionPreference="Stop"

$ProjectRoot="C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"
$SdkRoot=Join-Path $ProjectRoot "docs\sdk"
$ScriptRoot=Join-Path $ProjectRoot "scripts\sdk"
$OutputFile=Join-Path $SdkRoot "04-SDK-ENGINEERING-STANDARDS-v1.md"

New-Item -ItemType Directory -Force -Path $SdkRoot | Out-Null

if(Test-Path $OutputFile){Remove-Item $OutputFile -Force}

$parts=@(
"create-sdk-engineering-standards-part1.ps1",
"create-sdk-engineering-standards-part2.ps1",
"create-sdk-engineering-standards-part3.ps1"
)

foreach($part in $parts){
    $script=Join-Path $ScriptRoot $part
    if(!(Test-Path $script)){throw "Missing required script: $script"}
    Write-Host "Executing $part..."
    & $script
    if($LASTEXITCODE -ne $null -and $LASTEXITCODE -ne 0){throw "$part failed."}
}

if(!(Test-Path $OutputFile)){throw "SDK Engineering Standards document was not generated."}

Write-Host ""
Write-Host "============================================================="
Write-Host "SUCCESS"
Write-Host "============================================================="
Write-Host "Generated:"
Write-Host $OutputFile
