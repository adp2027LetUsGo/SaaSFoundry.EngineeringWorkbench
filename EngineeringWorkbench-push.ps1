#requires -Version 7.0

<#
    EngineeringWorkbench-push.ps1
    SaaSFoundry.EngineeringWorkbench

    DAILY GIT CHECKPOINT

    Usage:
        .\EngineeringWorkbench-push.ps1

    No user input is required.

    Automatically:
        - uses the canonical repository
        - detects the current branch
        - stages all changes
        - creates a timestamped checkpoint commit
        - pushes the current branch to origin

    Never:
        - creates branches
        - merges
        - force-pushes
        - resets
        - cleans files
#>

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

# ============================================================
# CANONICAL CONFIGURATION
# ============================================================

$RepoPath = "C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"
$Remote = "origin"
$ProjectName = "SaaSFoundry.EngineeringWorkbench"

# ============================================================
# REPOSITORY VALIDATION
# ============================================================

if (-not (Test-Path -LiteralPath $RepoPath -PathType Container)) {
    Write-Host "ERROR: Repository path does not exist." -ForegroundColor Red
    Write-Host $RepoPath
    exit 1
}

Set-Location -LiteralPath $RepoPath

$GitRoot = (git rev-parse --show-toplevel).Trim()

if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($GitRoot)) {
    Write-Host "ERROR: Unable to determine Git repository root." -ForegroundColor Red
    exit 1
}

$GitRoot = (Resolve-Path -LiteralPath $GitRoot).Path
$ExpectedRoot = (Resolve-Path -LiteralPath $RepoPath).Path

if ($GitRoot -ne $ExpectedRoot) {
    Write-Host "ERROR: Git root does not match the canonical repository." -ForegroundColor Red
    Write-Host "Git root : $GitRoot"
    Write-Host "Expected : $ExpectedRoot"
    exit 1
}

$Branch = (git branch --show-current).Trim()

if ([string]::IsNullOrWhiteSpace($Branch)) {
    Write-Host "ERROR: Detached HEAD detected." -ForegroundColor Red
    exit 1
}

$RemoteUrl = (git remote get-url $Remote 2>$null).Trim()

if ([string]::IsNullOrWhiteSpace($RemoteUrl)) {
    Write-Host "ERROR: Remote '$Remote' is not configured." -ForegroundColor Red
    exit 1
}

# ============================================================
# CHECK WORKING TREE
# ============================================================

$Status = @(git status --porcelain)

if ($Status.Count -eq 0) {
    Write-Host "[$ProjectName] Nothing to commit. Working tree clean." -ForegroundColor Green
    exit 0
}

# ============================================================
# AUTOMATIC TIMESTAMP
# ============================================================

$Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm zzz"

# ============================================================
# GENERIC SEMANTIC CHECKPOINT
# ============================================================

$CommitMessage = "chore(checkpoint): $ProjectName update - $Timestamp"

# ============================================================
# STAGE
# ============================================================

git add -A

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: git add failed." -ForegroundColor Red
    exit 1
}

# ============================================================
# COMMIT
# ============================================================

git commit -m $CommitMessage

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: git commit failed." -ForegroundColor Red
    exit 1
}

$CommitHash = (git rev-parse --short HEAD).Trim()

# ============================================================
# PUSH
# ============================================================

git push $Remote $Branch

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: git push failed." -ForegroundColor Red
    exit 1
}

# ============================================================
# RESULT
# ============================================================

Write-Host ""
Write-Host "============================================================" -ForegroundColor Green
Write-Host " GIT CHECKPOINT COMPLETED" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host "Project : $ProjectName"
Write-Host "Branch  : $Branch"
Write-Host "Commit  : $CommitHash"
Write-Host "Time    : $Timestamp"
Write-Host "Remote  : $Remote"
Write-Host "Message : $CommitMessage"
Write-Host ""
