# ============================================================================
# SaaSFoundry EngineeringWorkbench
# Script: create-canon-bootstrap.ps1
# Purpose: Bootstrap the Engineering Canon infrastructure.
# Version: 1.0
# ============================================================================

$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# ROOT
# ---------------------------------------------------------------------------

$ProjectRoot = "C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"

$DocsRoot      = Join-Path $ProjectRoot "docs"
$CanonRoot     = Join-Path $DocsRoot "canon"

$ScriptsRoot   = Join-Path $ProjectRoot "scripts"
$CanonScripts  = Join-Path $ScriptsRoot "canon"

# ---------------------------------------------------------------------------
# UTF8
# ---------------------------------------------------------------------------

$Utf8 = New-Object System.Text.UTF8Encoding($false)

function Write-Utf8File
{
    param(
        [string]$Path,
        [string]$Content
    )

    [System.IO.File]::WriteAllText($Path,$Content,$Utf8)
}

# ---------------------------------------------------------------------------
# LOG
# ---------------------------------------------------------------------------

function Write-Step
{
    param([string]$Message)

    Write-Host ""
    Write-Host "==================================================================" -ForegroundColor DarkGray
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "==================================================================" -ForegroundColor DarkGray
}

# ---------------------------------------------------------------------------
# CREATE DIRECTORY
# ---------------------------------------------------------------------------

function Ensure-Directory
{
    param([string]$Path)

    if(!(Test-Path $Path))
    {
        New-Item `
            -ItemType Directory `
            -Path $Path `
            -Force | Out-Null

        Write-Host "Created Directory : $Path" -ForegroundColor Green
    }
    else
    {
        Write-Host "Exists            : $Path" -ForegroundColor Yellow
    }
}

# ---------------------------------------------------------------------------
# HEADER TEMPLATE
# ---------------------------------------------------------------------------

function New-CanonicalHeader
{
param(
[string]$Document,
[string]$Classification
)

@"
# SaaSFoundry.EngineeringWorkbench

Document:
$Document

Version:
1.0

Status:
DRAFT

Classification:
$Classification

Authority:
TBD

Owner:
SaaSFoundry Engineering

------------------------------------------------------------------------------

Content Pending.

------------------------------------------------------------------------------
"@
}

# ---------------------------------------------------------------------------
# SCRIPT TEMPLATE
# ---------------------------------------------------------------------------

function New-ScriptTemplate
{
param(
[string]$ScriptName,
[string]$Purpose
)

@"
# ============================================================================
# SaaSFoundry EngineeringWorkbench
#
# Script:
# $ScriptName
#
# Purpose:
# $Purpose
#
# Version:
# 1.0
#
# ============================================================================

`$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================================="
Write-Host "$ScriptName"
Write-Host "============================================================="
Write-Host ""

Write-Host "Not Implemented Yet." -ForegroundColor Yellow
"@
}

# ---------------------------------------------------------------------------
# README
# ---------------------------------------------------------------------------

$Readme = @"
# EngineeringWorkbench Canon

This directory contains the constitutional documentation for the
SaaSFoundry EngineeringWorkbench.

Reading Order

00 System Canon

01 Product Requirements

02 Engineering Vision

03 Architecture Blueprint

04 Architecture Freeze

05 Engineering Standards

06 Current State

07 Roadmap

08 Backlog

09 Handoff

10 ADR Index

11 Engineering Knowledge
"@

Write-Step "Creating Folder Structure"

Ensure-Directory $DocsRoot
Ensure-Directory $CanonRoot

Ensure-Directory $ScriptsRoot
Ensure-Directory $CanonScripts

Write-Step "Creating README"

Write-Utf8File `
    -Path (Join-Path $CanonRoot "README.md") `
    -Content $Readme

Write-Step "Creating Canon Documents"

$Documents = @(

@{
Name="00-SYSTEM-CANON-v1.md"
Document="00-SYSTEM-CANON"
Classification="CONSTITUTIONAL"
},

@{
Name="01-PRODUCT-REQUIREMENTS-v1.md"
Document="01-PRODUCT-REQUIREMENTS"
Classification="CONSTITUTIONAL"
},

@{
Name="02-ENGINEERING-VISION-v1.md"
Document="02-ENGINEERING-VISION"
Classification="CONSTITUTIONAL"
},

@{
Name="03-ARCHITECTURE-BLUEPRINT-v1.md"
Document="03-ARCHITECTURE-BLUEPRINT"
Classification="CONSTITUTIONAL"
},

@{
Name="04-ARCHITECTURE-FREEZE-v1.md"
Document="04-ARCHITECTURE-FREEZE"
Classification="CONSTITUTIONAL"
},

@{
Name="05-ENGINEERING-STANDARDS-v1.md"
Document="05-ENGINEERING-STANDARDS"
Classification="NORMATIVE"
},

@{
Name="06-CURRENT-STATE-v1.md"
Document="06-CURRENT-STATE"
Classification="OPERATIONAL"
},

@{
Name="07-ROADMAP-v1.md"
Document="07-ROADMAP"
Classification="OPERATIONAL"
},

@{
Name="08-BACKLOG-v1.md"
Document="08-BACKLOG"
Classification="OPERATIONAL"
},

@{
Name="09-HANDOFF-v1.md"
Document="09-HANDOFF"
Classification="OPERATIONAL"
},

@{
Name="10-ADR-INDEX-v1.md"
Document="10-ADR-INDEX"
Classification="REFERENCE"
},

@{
Name="11-ENGINEERING-KNOWLEDGE-v1.md"
Document="11-ENGINEERING-KNOWLEDGE"
Classification="REFERENCE"
}

)

foreach($doc in $Documents)
{
    $Path = Join-Path $CanonRoot $doc.Name

    Write-Utf8File `
        -Path $Path `
        -Content (New-CanonicalHeader `
                    -Document $doc.Document `
                    -Classification $doc.Classification)

    Write-Host "Created : $($doc.Name)" -ForegroundColor Green
}

Write-Step "Creating Script Templates"

$Scripts = @(

@{
Name="create-system-canon.ps1"
Purpose="Generates the System Canon."
},

@{
Name="create-product-requirements.ps1"
Purpose="Generates the Product Requirements."
},

@{
Name="create-engineering-vision.ps1"
Purpose="Generates the Engineering Vision."
},

@{
Name="create-architecture-blueprint.ps1"
Purpose="Generates the Architecture Blueprint."
},

@{
Name="create-architecture-freeze.ps1"
Purpose="Generates the Architecture Freeze."
},

@{
Name="create-engineering-standards.ps1"
Purpose="Generates the Engineering Standards."
},

@{
Name="create-current-state.ps1"
Purpose="Generates the Current State."
},

@{
Name="create-roadmap.ps1"
Purpose="Generates the Roadmap."
},

@{
Name="create-backlog.ps1"
Purpose="Generates the Backlog."
},

@{
Name="create-handoff.ps1"
Purpose="Generates the Handoff."
},

@{
Name="create-adr-index.ps1"
Purpose="Generates the ADR Index."
},

@{
Name="create-engineering-knowledge.ps1"
Purpose="Generates the Engineering Knowledge."
}

)

foreach($script in $Scripts)
{
    $Path = Join-Path $CanonScripts $script.Name

    Write-Utf8File `
        -Path $Path `
        -Content (New-ScriptTemplate `
                    -ScriptName $script.Name `
                    -Purpose $script.Purpose)

    Write-Host "Created : $($script.Name)" -ForegroundColor Green
}

Write-Step "Bootstrap Completed"

Write-Host ""
Write-Host "Engineering Canon Bootstrap Complete." -ForegroundColor Green
Write-Host ""
Write-Host "Next Step:" -ForegroundColor Cyan
Write-Host ""
Write-Host "create-system-canon.ps1"
Write-Host ""