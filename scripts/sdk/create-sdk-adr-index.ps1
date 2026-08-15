# ============================================================================
# SaaSFoundry SDK
# Script: create-sdk-adr-index.ps1
# Purpose: Orchestrates complete generation of the SDK ADR Index document.
# Version: 1.0
# ============================================================================

$ErrorActionPreference="Stop"

$ProjectRoot="C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"
$SdkRoot=Join-Path $ProjectRoot "docs\sdk"
$ScriptRoot=Join-Path $ProjectRoot "scripts\sdk"
$OutputFile=Join-Path $SdkRoot "08-SDK-ADR-INDEX-v1.md"

New-Item -ItemType Directory -Force -Path $SdkRoot | Out-Null

if(Test-Path $OutputFile){Remove-Item $OutputFile -Force}

$parts=@(
"create-sdk-adr-index-part1.ps1",
"create-sdk-adr-index-part2.ps1",
"create-sdk-adr-index-part3.ps1"
)

foreach($part in $parts){
    $script=Join-Path $ScriptRoot $part
    if(!(Test-Path $script)){throw "Missing required script: $script"}

    Write-Host "Executing $part..."
    & $script

    if($LASTEXITCODE -ne $null -and $LASTEXITCODE -ne 0){
        throw "$part failed."
    }
}

if(!(Test-Path $OutputFile)){
    throw "SDK ADR Index document was not generated."
}

Write-Host ""
Write-Host "============================================================="
Write-Host "SUCCESS"
Write-Host "============================================================="
Write-Host "Generated:"
Write-Host $OutputFile
