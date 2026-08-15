# Architecture Freeze Amendment: Runtime Materialization (v1.1.3)

## 1. Purpose
This amendment formally defines the canonical Runtime Materialization architecture for the Generated Product Runtime. It resolves the architecture gap where products (like VibeStock) were scaffolded but not automatically materialized by the Factory.

## 2. Canonical Pipeline
The canonical generation and materialization pipeline is defined as:

1. **Product Composition**: A Product Manifest defines the target Cells and their assigned plugin capabilities.
2. **Cell Composition**: The generic mapping of capabilities to physical `.Cell` projects (e.g., `Core`, `System`).
3. **Capability Selection**: The MissionPlanner selects capabilities based on the Product Composition.
4. **Engineering Plan**: The Planner outputs an Engineering Plan defining generation dependencies.
5. **Artifact Generation**: The GenerationEngine invokes Plugin Generators to emit artifacts to `output/`.
6. **Artifact Validation**: The ValidationEngine verifies intermediate artifacts.
7. **Artifact Packaging**: The ArtifactWriter packages output artifacts.
8. **Runtime Materialization**: A new canonical materializer projects artifacts into the target Generated Product Runtime `.Cell` directories and generates the runtime host.
9. **Generated Product Runtime**: The target execution boundary.

## 3. Product and Cell Composition
The Product Definition is the canonical input. It maps plugins (API, Authentication, Observability, Persistence) to Cells (Core, Ingestor, Bridge, System). This manifest must be product-agnostic so future products (e.g., `FixedAssetManager`) can use the same pipeline.

## 4. Program.cs and Hosting Ownership
**Owner**: The Factory (specifically the Runtime Materialization stage).
- Plugins **MUST NOT** independently generate or overwrite `Program.cs`.
- The Factory owns `Program.cs`, `WebApplication.CreateBuilder`, Kestrel configuration, and the middleware pipeline.
- It supports NativeAOT and ASP.NET Core `Microsoft.NET.Sdk.Web` hosting out-of-the-box.

## 5. Plugin Contribution Model
- Plugins generate registration fragments (e.g., `AddObservability()`, `UseAuthentication()`) in partial classes or extension methods inside `Generated/<Plugin>/`.
- The Factory's Host Generator discovers these or explicitly calls them in the composed `Program.cs`.

## 6. Project Configuration Ownership
**Owner**: The Factory.
- The Factory owns `.csproj` and generates `PackageReference`, `ProjectReference`, `FrameworkReference`, and `NativeAOT` configurations based on generic requirements declared by the capabilities.

## 7. Determinism and Traceability
- **Determinism**: The materialization process guarantees equivalent output for identical Product Definitions and Plugin versions. Timestamps, random IDs, and machine paths are forbidden.
- **Traceability**: All generated code must be traceable to a `GeneratedArtifactDescriptor` via `CanonReference`, `CapabilityId`, and `ValidationEvidenceId`.

## 8. Regeneration and Boundaries
- **Regeneration**: Idempotent. Re-running materialization safely replaces previous generated artifacts without manual edits.
- **Boundaries**: Anything within a `Generated/` folder is owned by the Factory. User-owned source code must live outside `Generated/`.

## 9. VibeStock and Future Product Reuse
The generic Factory Materialization must not contain VibeStock-specific logic (e.g., Shopify, Commerce). Product-specific capabilities must be specialized and injected into the pipeline without altering the core Factory infrastructure.

## 10. Required Factory Amendment
The Factory must implement a new Runtime Materializer responsible for resolving target paths, injecting generic host definitions (`Program.cs`), and orchestrating project file (`.csproj`) dependencies.

## 11. Core and SDK
- **Core**: Unchanged.
- **SDK**: Unchanged.
## 12. Planning and Contributions Update (v1.1.4)

This architecture is further refined by Architecture Freeze v1.1.4, which introduces the Factory Code-Generation Planner and implicitly coordinates the Runtime Registration Contribution Model without altering frozen Core contracts.
