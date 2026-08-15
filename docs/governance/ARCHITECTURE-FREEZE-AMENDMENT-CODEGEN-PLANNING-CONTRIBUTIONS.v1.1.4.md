# Architecture Freeze Amendment: Code Generation Planning and Contributions (v1.1.4)

## 1. Purpose
This amendment resolves the architecture gaps identified in Stage 9N.4P. It formally defines the Code-Generation Planner (distinct from the agent-execution MissionPlanner) and the Runtime Registration Contribution Model, without requiring any modifications to frozen Core or SDK contracts.

## 2. Code Generation Planner
- **Owner**: Factory (`SaaSFoundry.EngineeringWorkbench.Builder`).
- **Responsibility**: Translates a generic `ProductDefinition` into a deterministic `CodeGenerationPlan`.
- **Why**: The existing `MissionPlanner` is an agent-execution DAG planner. Code generation requires a specialized planner to resolve product/cell mappings to capabilities, capability dependencies, and generation order.

## 3. Product Definition and Cell Composition
- **Owner**: Factory (Data/Configuration model).
- **Structure**: A canonical manifest (e.g., JSON) defining the `Product` (e.g., VibeStock), its `Cells` (e.g., Core.Cell, System.Cell), and the `Capabilities` assigned to each cell.
- **VibeStock Boundary**: The Product Definition is purely configuration. Factory source code remains completely agnostic of "VibeStock" or "FixedAssetManager".

## 4. Capability Selection and Routing
- **Owner**: Factory Code Generation Planner.
- **Routing**: The Planner matches the capabilities requested in the Product Definition against registered `IPluginCapability` providers. It determines target output directories and capability generation order.

## 5. Runtime Registration Contribution Model
- **Mechanism**: **Generated-runtime-only convention** driven by Factory Planner metadata.
- **Why**: The `IPluginCapability` contract in Core is frozen and does not support returning typed C# fragments.
- **How it works**:
  1. The Planner knows exactly which Capabilities are routed to a specific Cell.
  2. The Factory's Host Generator emits a `Program.cs` that explicitly invokes deterministic extension methods based on those capabilities (e.g., `builder.Services.Add{CapabilityName}()`).
  3. The Plugin Capability, during `GenerateArtifactsAsync`, is responsible for generating the C# source file containing that specific extension method into its artifact output.
- **Result**: No reflection, no dynamic discovery, and no modifications to Core. The Factory explicitly knows the contributions from the Planner, and the plugins fulfill the generated contract.

## 6. Artifact Ownership
- **Factory**: Owns `Program.cs`, the `.csproj` file, host configuration, the `CodeGenerationPlan`, and the materialization boundaries.
- **Plugins**: Own their specific capability runtime fragments (the extension methods) and functional source code.

## 7. Pipeline Ownership and Boundary
- **Planner**: "What capabilities go to what Cell?" (Factory)
- **Generator**: "Produce the artifact." (Plugins via `GenerationEngine`)
- **Validator**: "Is the artifact valid?" (Factory via `ValidationEngine`)
- **Packager**: "Prepare the artifact." (Factory via `ArtifactWriter` / Packaging)
- **Materializer**: "Where does the artifact belong in the target runtime?" (Factory)
- **Host Generator**: "How are approved runtime contributions composed into Program.cs?" (Factory)

## 8. Dependency Direction
`Factory` -> `Plugins` -> `SDK` -> `Core`.
- Core depends on nothing.
- SDK depends on Core.
- Plugins depend on SDK and Core.
- Factory depends on Plugins, SDK, and Core.
- The Runtime Contribution Model exists purely between the Factory (generating the calls) and the Plugins (generating the implementations).

## 9. Future Product Reuse
This model is 100% reusable. `FixedAssetManager` can be generated simply by supplying a different Product Definition manifest. No Factory source code branching is required.

## 10. Summary of Freezes
- **Core**: Unchanged.
- **SDK**: Unchanged.
- **Plugins**: Unchanged structurally (must simply generate matching extension methods).
- **Factory**: Requires implementation of the `CodeGenerationPlanner`, `ProductDefinition` model, and `HostGenerator`.


## Amendment v1.1.5 Traceability

Architecture Freeze v1.1.5 defines that Runtime Registration Metadata is provided by the Factory (e.g. \CapabilityRegistrationMetadata\) because \IPluginCapability\ is frozen and cannot declare this metadata. See \ARCHITECTURE-FREEZE-AMENDMENT-RUNTIME-REGISTRATION-METADATA.v1.1.5.md\.
