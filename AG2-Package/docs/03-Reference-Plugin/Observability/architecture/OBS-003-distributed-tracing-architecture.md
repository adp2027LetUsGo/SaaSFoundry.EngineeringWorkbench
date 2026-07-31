# Distributed Tracing Architecture

Document ID: OBS-003

Version: 0.1

Status: Draft


## Purpose

Define distributed tracing across SaaS-Foundry Cells.


## Standard

OpenTelemetry Distributed Tracing.


## Trace Model


Execution:

Request

↓

Cell

↓

Service

↓

Database

↓

External System


All operations participate in the same trace.


## Trace Context

Required:

- TraceId
- SpanId
- W3C Trace Context


## Activity Source

Each Cell must define its ActivitySource.


Example:

AHS.Inventory.Cell

AHS.System.Cell

AHS.Bridge.Cell


## Supported Operations

Tracing required for:

- HTTP
- gRPC
- background jobs
- messaging
- external API calls


## Goals

Tracing must answer:

- What happened?
- Where?
- When?
- Why?


## Implementation Baseline

- System.Diagnostics.Activity
- ActivitySource
- OpenTelemetry SDK


## Status

Draft
