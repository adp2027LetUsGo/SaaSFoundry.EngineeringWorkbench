# AHS Distributed Tracing Standard

## Purpose

Trace execution across Cells.

Example:

API
 |
Ingestor.Cell
 |
Core.Cell
 |
Bridge.Cell
 |
External Service


## Rules

Every Cell MUST define:

ActivitySource

Naming:

AHS.{CellName}
