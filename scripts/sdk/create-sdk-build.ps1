# ============================================================================
# SaaSFoundry SDK
# Script: create-sdk-build.ps1
# Purpose: Regenerates the complete SDK Canon v1.0
# Version: 1.0
# ============================================================================

$ErrorActionPreference="Stop"

$ProjectRoot="C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"
$ScriptRoot=Join-Path $ProjectRoot "scripts\sdk"
$SdkRoot=Join-Path $ProjectRoot "docs\sdk"

$generators=@(
"create-sdk-handoff.ps1",
"create-sdk-vision.ps1",
"create-sdk-architecture.ps1",
"create-sdk-architecture-freeze.ps1",
"create-sdk-engineering-standards.ps1",
"create-sdk-current-state.ps1",
"create-sdk-roadmap.ps1",
"create-sdk-backlog.ps1",
"create-sdk-adr-index.ps1",
"create-sdk-engineering-knowledge.ps1"
)

$documents=@(
"00-SDK-HANDOFF-v1.md",
"01-SDK-VISION-v1.md",
"02-SDK-ARCHITECTURE-v1.md",
"03-SDK-ARCHITECTURE-FREEZE-v1.md",
"04-SDK-ENGINEERING-STANDARDS-v1.md",
"05-SDK-CURRENT-STATE-v1.md",
"06-SDK-ROADMAP-v1.md",
"07-SDK-BACKLOG-v1.md",
"08-SDK-ADR-INDEX-v1.md",
"09-SDK-ENGINEERING-KNOWLEDGE-v1.md"
)

$sw=[System.Diagnostics.Stopwatch]::StartNew()

foreach($g in $generators){
    $script=Join-Path $ScriptRoot $g
    if(!(Test-Path $script)){throw "Missing generator: $script"}
    Write-Host "Executing $g..."
    & $script
}

foreach($d in $documents){
    if(!(Test-Path (Join-Path $SdkRoot $d))){
        throw "Missing document: $d"
    }
}

$manifest=Join-Path $SdkRoot "SDK-MANIFEST.md"
$summary=Join-Path $SdkRoot "SDK-SUMMARY.md"

Set-Content $manifest "# SDK Manifest`r`n`r`nGenerated: $(Get-Date -Format u)" -Encoding UTF8
Add-Content $manifest ""
$documents | ForEach-Object { Add-Content $manifest "- $_" }

Set-Content $summary @"
# SDK Summary

Status: SUCCESS

Documents: $($documents.Count)

Elapsed: $([Math]::Round($sw.Elapsed.TotalSeconds,2)) seconds
"@ -Encoding UTF8

$sw.Stop()

Write-Host ""
Write-Host "============================================================="
Write-Host "SDK CANON BUILD SUCCEEDED"
Write-Host "============================================================="
Write-Host "Generated:"
Write-Host $SdkRoot
