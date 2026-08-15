# SaaSFoundry.EngineeringWorkbench
# NETWORK POLICY

Version: 1.0

Status:
CANONICAL

Last Updated:
2026-08-03

Authority:
EngineeringWorkbench System Canon

---

# Purpose

This document defines the official Network Access Policy for Antigravity 2.0.

Network access exists exclusively to validate implementations and verify official technical information.

Network access SHALL NEVER define architecture.

The EngineeringWorkbench architecture is defined exclusively by the local Engineering Canon.

---

# Engineering Authority

The following authority hierarchy SHALL always be respected.

Priority 1

Local Engineering Canon

Priority 2

Official Documentation

Priority 3

Community Information

Whenever two sources disagree, the higher priority source prevails.

---

# Priority 1
## Local Engineering Canon

Antigravity SHALL always consult the local repository before accessing the Internet.

Authoritative engineering sources include

• AG2 Package

• System Canon

• Architecture Documents

• Engineering Standards

• Reference Plugin Standard

• Plugin Generation Specification

• Engineering Program Status

• Engineering Chronogram

• Golden Reference Plugin

• Current Repository Source Code

The local repository is the single source of architectural truth.

---

# Priority 2
## Official Documentation

Official documentation MAY be consulted to validate implementation details.

Approved sources

Microsoft Learn

https://learn.microsoft.com/

ASP.NET Core

https://learn.microsoft.com/aspnet/core/

.NET

https://learn.microsoft.com/dotnet/

NativeAOT

https://learn.microsoft.com/dotnet/core/deploying/native-aot/

NuGet

https://www.nuget.org/

Official GitHub Repositories

https://github.com/dotnet/runtime

https://github.com/dotnet/aspnetcore

https://github.com/DapperLib/Dapper

https://github.com/npgsql/npgsql

PostgreSQL

https://www.postgresql.org/docs/

Shopify Developers

https://shopify.dev/

OpenTelemetry

https://opentelemetry.io/docs/

OpenAI

https://platform.openai.com/docs

Google AI

https://ai.google.dev/

Only official documentation may be used.

---

# Priority 3
## Community Information

Community resources MAY be consulted only when

• Official documentation does not exist.

AND

• The information can be verified against official sources.

Community information SHALL NEVER become engineering authority.

---

# Allowed Uses

Network access MAY be used for

• API documentation

• SDK documentation

• Breaking changes

• Package versions

• Compatibility verification

• NativeAOT verification

• Official implementation guidance

• Official samples

• Performance recommendations

• Security recommendations

---

# Forbidden Uses

Network access SHALL NOT be used for

Architecture decisions

Platform redesign

Replacing System Canon

Replacing Engineering Standards

Changing Core contracts

Changing Plugin Runtime

Changing Validation Engine

Changing Packaging Engine

Copying third-party architectures

Copying external production code

Following blog recommendations without verification

Introducing experimental frameworks

---

# Plugin Development Policy

When generating plugins

Architecture SHALL come from

Observability Reference Plugin

Engineering Standards

System Canon

Internet SHALL ONLY verify

Technology usage

SDK syntax

API signatures

Framework compatibility

Package compatibility

---

# NativeAOT Policy

When official documentation conflicts with NativeAOT requirements

NativeAOT compatibility SHALL prevail.

Dynamic features SHALL NOT be introduced.

Reflection SHALL NOT be introduced.

Assembly scanning SHALL NOT be introduced.

---

# Conflict Resolution

If any conflict exists between

Repository

and

Official Documentation

Antigravity SHALL

STOP IMPLEMENTATION

Generate an Engineering Decision Report

Explain the conflict

Request human approval

Implementation SHALL NOT continue automatically.

---

# Security Policy

Only HTTPS sources are allowed.

No unofficial package repositories.

No unofficial mirrors.

No downloaded source code may become part of the project without explicit approval.

---

# Performance Policy

Internet SHALL NOT be used to introduce optimization techniques that violate

Determinism

NativeAOT

Zero Reflection

Architecture Freeze

---

# Final Engineering Rule

Architecture is defined locally.

Implementation is validated externally.

Internet validates engineering.

The repository defines engineering.

---

End of Document