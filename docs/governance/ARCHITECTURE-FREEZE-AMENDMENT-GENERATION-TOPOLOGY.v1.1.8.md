# ARCHITECTURE FREEZE AMENDMENT: GENERATION TOPOLOGY CONTEXT v1.1.8

## 1. Purpose
Resolves the architecture gap regarding how the generic generated transport capability determines the inter-cell communication topology and the target Cell identity, without modifying frozen runtime Core contracts.

## 2. Generation Context Ownership
The generation context is owned strictly by the SDK and Factory (`SaaSFoundry.EngineeringWorkbench.Builder`). `ProductDefinition` and `CodeGenerationPlan` are the canonical sources of configuration during generation.

## 3. Frozen Core Boundary
`IPluginExecutionContext` (frozen in Core) remains unmodified. The Core execution context receives configuration strictly via its `Arguments` array, maintaining compatibility with the deterministic extraction phase. No new Core interfaces are introduced.

## 4. Product Definition Topology
`ProductDefinition` is expanded to include a generic `Communications` graph. This models the canonical inter-cell topology abstractly:
- `SourceCell`: The originating cell.
- `DestinationCell`: The receiving cell.
- `Mode`: The communication mode (e.g., `Bidirectional`, `Outbound`).

## 5. Cell Routing
VibeStock Cell routing (e.g., Core ↔ Ingestor, Core → System) is modeled entirely as configuration within `product.json`. Generative plugins read this graph without hardcoding VibeStock-specific product logic, ensuring they remain reusable.

## 6. Target Cell Identity
The Target Cell Identity (`CellId`) and canonical `ProductDefinition` graph are passed by the `RuntimeMaterializer` to the capability generator via `DefaultExecutionContext` arguments (e.g., `--target-cell` and `--topology-path`), which point to deterministic, staged JSON configurations.

## 7. Determinism and NativeAOT
- All file extractions and JSON serialization are deterministic.
- No timestamps, random identifiers, or environment-specific paths are used in the generated capability logic.
- Reflection remains prohibited; the generation phase continues to emit reflection-free, NativeAOT-compatible C# artifacts.
