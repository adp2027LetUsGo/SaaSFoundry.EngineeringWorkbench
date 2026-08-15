# ============================================================================
# SaaSFoundry SDK
# Script: create-sdk-handoff-part1.ps1
# Purpose: Generates Part 1 of the SDK Handoff document.
# Version: 1.0
# ============================================================================

$ErrorActionPreference = "Stop"

$ProjectRoot = "C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench"
$SdkRoot     = Join-Path $ProjectRoot "docs\sdk"
$OutputFile  = Join-Path $SdkRoot "00-SDK-HANDOFF-v1.md"

New-Item -ItemType Directory -Force -Path $SdkRoot | Out-Null

$content = @"
# SaaSFoundry SDK

# SDK HANDOFF

------------------------------------------------------------------------------
Document:
00-SDK-HANDOFF

Version:
1.0

Status:
ACTIVE

Classification:
CONSTITUTIONAL

Authority:
EngineeringWorkbench System Canon

Owner:
SaaSFoundry Engineering

------------------------------------------------------------------------------

# 1. Executive Summary

This document provides the engineering context required to begin
development of the official SaaSFoundry SDK.

The SDK is the official developer framework for building production
plugins for the EngineeringWorkbench.

------------------------------------------------------------------------------

# 2. Mission

Develop a stable, strongly typed, Native AOT compatible SDK that enables
developers to build plugins with minimal boilerplate while preserving the
frozen EngineeringWorkbench architecture.

------------------------------------------------------------------------------

# 3. Relationship with EngineeringWorkbench

EngineeringWorkbench is the host platform.

The SDK is the developer-facing framework.

Plugins are built with the SDK and executed by the EngineeringWorkbench Runtime.

------------------------------------------------------------------------------

# 4. Current Platform Baseline

Architecture Status:

FROZEN

Engineering Canon:

Version 1.0

Reference Plugin:

Observability

Primary Target:

Developer productivity without compromising architectural integrity.

------------------------------------------------------------------------------
"@

Set-Content -Path $OutputFile -Value $content -Encoding UTF8

Write-Host ""
Write-Host "Generated:"
Write-Host $OutputFile
