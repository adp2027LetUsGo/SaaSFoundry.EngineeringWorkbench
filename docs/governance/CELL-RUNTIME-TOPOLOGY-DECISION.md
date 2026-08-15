# Cell Runtime Topology Decision

## Status
APPROVED

## Macro-Cells
The canonical VibeStock macro-cell model is composed of `Core.Cell`, `Ingestor.Cell`, `Bridge.Cell`, and `System.Cell`. Each Cell is an independently deployable runtime process/container.

## Core.Cell
**Primary Responsibility:** VibeStock core commerce/inventory business domain.
- Core business operations (Products, Variants, Inventory, Pricing).
- **Generated Artifacts:** Core domain API, Authentication, Observability, Persistence.
- **Database:** `Core.Cell` PostgreSQL database.

## Ingestor.Cell
**Primary Responsibility:** Upload, Parsing, AI Mapping, Normalization, Preview.
- **Generated Artifacts:** Ingestion API, Authentication, Observability, Persistence.
- **Database:** `Ingestor.Cell` PostgreSQL database.

## Bridge.Cell
**Primary Responsibility:** Shopify GraphQL, Validation, Rate Limiting, Retry, and external commerce integration.
- **Generated Artifacts:** Shopify integration API, Authentication, Observability, Persistence.
- **Database:** `Bridge.Cell` PostgreSQL database.

## System.Cell
**Primary Responsibility:** Background Jobs, Job Scheduling, Job Execution, Audit, and system-level operations.
- **Generated Artifacts:** BackgroundProcessing workers/scheduler, System API, Authentication, Observability, Persistence.
- **Database:** `System.Cell` PostgreSQL database (owns the durable Job Queue).

## Generated Runtime Mapping
Each plugin artifact generator emits source artifacts that are compiled into their respective target Cell runtimes.

## Persistence Mapping
Generated persistence source code executes inside each Cell, connecting to that Cell's specific PostgreSQL database.

## Job Storage Mapping
The durable job queue is owned exclusively by `System.Cell` and is located in the `System.Cell` PostgreSQL database. The `IJobStorageCapability` concrete implementation executes inside the `System.Cell` runtime process.

## Background Processing Mapping
The BackgroundProcessing runtime (workers, job scheduler, IJobStorageCapability consumers) executes within the `System.Cell` process.

## API Mapping
Generated API runtime is compiled into the Cell that owns the corresponding business capabilities (e.g., Core domain API inside `Core.Cell`, Ingestion API inside `Ingestor.Cell`).

## Authentication Mapping
Authentication is a cross-cutting runtime infrastructure. The generated authentication middleware is included in every Cell exposing HTTP/API endpoints, establishing canonical `IdentityContext`, `TenantContext`, and `AuthorizationContext`.

## Observability Mapping
Observability is a cross-cutting concern. Generated observability runtime is embedded into every Cell. Distributed tracing spans across all Cells.

## Database-per-Cell
Each Cell owns its independent persistence runtime and PostgreSQL database.

## RLS
Row-Level Security (RLS) enforces tenant isolation within every Cell's PostgreSQL database.

## Inter-Cell Communication
Cross-Cell communication MUST use canonical inter-Cell mechanisms. Direct access to another Cell's database is strictly prohibited. If the exact inter-Cell mechanism (e.g. gRPC/HTTP) is not explicitly defined in the repository, it constitutes a GOVERNANCE GAP that must not be unilaterally invented (no brokers such as RabbitMQ or Kafka).

## Process Topology
```text
VibeStock
|
+-- Core.Cell
|   +-- Independent process/container
|   +-- Core domain runtime
|   +-- API runtime
|   +-- Authentication runtime
|   +-- Persistence runtime
|   +-- Observability runtime
|   +-- Core.Cell PostgreSQL
|
+-- Ingestor.Cell
|   +-- Independent process/container
|   +-- Ingestion runtime
|   +-- API runtime
|   +-- Authentication runtime
|   +-- Persistence runtime
|   +-- Observability runtime
|   +-- Ingestor.Cell PostgreSQL
|
+-- Bridge.Cell
|   +-- Independent process/container
|   +-- Shopify integration runtime
|   +-- API runtime where required
|   +-- Authentication runtime where required
|   +-- Persistence runtime
|   +-- Observability runtime
|   +-- Bridge.Cell PostgreSQL
|
+-- System.Cell
    +-- Independent process/container
    +-- BackgroundProcessing runtime
    +-- Job scheduler
    +-- Job workers
    +-- System API where required
    +-- Authentication runtime where required
    +-- Persistence runtime
    +-- Observability runtime
    +-- Audit
    +-- System.Cell PostgreSQL
    +-- Durable Job Queue
```

## VibeStock
VibeStock is the first commercial target application for the EngineeringWorkbench. The Workbench generates infrastructure artifacts that ultimately become part of VibeStock's executable runtime.

## Dependency Graph
```text
Target Application (VibeStock)
├── Depends on: Npgsql, Dapper.AOT (External NuGets)
├── Depends on: Generated Persistence Artifacts (Source)
├── Depends on: Generated API Artifacts (Source)
├── Depends on: Generated Authentication Artifacts (Source)
├── Depends on: Generated BackgroundProcessing Artifacts (Source)
├── Depends on: Generated Observability Artifacts (Source)
└── Depends on: SaaSFoundry.EngineeringWorkbench.Core (Platform Contracts)
```

## Architectural Invariants
- `SaaSFoundry.EngineeringWorkbench.Core` remains the Golden Reference. It contains canonical runtime platform contracts.
- Plugins remain purely authoring-time artifact generators.
- SDK remains authoring-time infrastructure with zero internal dependencies.

## Certification Impact
Runtime Persistence certification remains NOT YET, as the generated runtime implementation has not yet been executed against PostgreSQL.
