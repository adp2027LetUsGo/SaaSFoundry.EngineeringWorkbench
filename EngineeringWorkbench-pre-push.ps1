#requires -Version 7.0

<#
    EngineeringWorkbench-pre-push.ps1

    SaaSFoundry.EngineeringWorkbench

    PURPOSE
    -------
    Prepares EngineeringWorkbench-push.ps1 for execution.

    This script:
        1. Locates the canonical repository.
        2. Verifies EngineeringWorkbench-push.ps1 exists.
        3. Removes the Windows download/security block from it.
        4. Starts EngineeringWorkbench-push.ps1.

    DAILY USE
    ---------
        .\EngineeringWorkbench-pre-push.ps1

    This means the user does NOT need to remember:
        Unblock-File -LiteralPath "..."

    The actual push workflow remains in:
        EngineeringWorkbench-push.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

# ============================================================
# CANONICAL CONFIGURATION
# ============================================================

$RepoPath = "C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"
$PushScript = Join-Path $RepoPath "EngineeringWorkbench-push.ps1"

# ============================================================
# VALIDATE REPOSITORY
# ============================================================

if (-not (Test-Path -LiteralPath $RepoPath -PathType Container)) {
    Write-Host "ERROR: EngineeringWorkbench repository not found." -ForegroundColor Red
    Write-Host $RepoPath
    exit 1
}

if (-not (Test-Path -LiteralPath $PushScript -PathType Leaf)) {
    Write-Host "ERROR: EngineeringWorkbench-push.ps1 not found." -ForegroundColor Red
    Write-Host $PushScript
    exit 1
}

# ============================================================
# UNBLOCK PUSH SCRIPT
# ============================================================

Unblock-File -LiteralPath $PushScript

# ============================================================
# EXECUTE PUSH SCRIPT
# ============================================================

& $PushScript

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
