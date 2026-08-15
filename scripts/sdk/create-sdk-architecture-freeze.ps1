# ============================================================================
# SaaSFoundry SDK
# Script: create-sdk-architecture-freeze.ps1
# Purpose: Orchestrates complete generation of the SDK Architecture Freeze.
# Version: 1.0
# ============================================================================

$ErrorActionPreference="Stop"

$ProjectRoot="C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"
$SdkRoot=Join-Path $ProjectRoot "docs\sdk"
$ScriptRoot=Join-Path $ProjectRoot "scripts\sdk"
$OutputFile=Join-Path $SdkRoot "03-SDK-ARCHITECTURE-FREEZE-v1.md"

New-Item -ItemType Directory -Force -Path $SdkRoot | Out-Null

if(Test-Path $OutputFile){Remove-Item $OutputFile -Force}

$parts=@(
"create-sdk-architecture-freeze-part1.ps1",
"create-sdk-architecture-freeze-part2.ps1",
"create-sdk-architecture-freeze-part3.ps1"
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
    throw "SDK Architecture Freeze document was not generated."
}

Write-Host ""
Write-Host "============================================================="
Write-Host "SUCCESS"
Write-Host "============================================================="
Write-Host "Generated:"
Write-Host $OutputFile
