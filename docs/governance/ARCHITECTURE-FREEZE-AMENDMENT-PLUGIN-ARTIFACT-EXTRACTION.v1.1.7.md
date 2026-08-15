# Architecture Freeze Amendment v1.1.7: Plugin Artifact Extraction Boundary

## 1. Context and Problem Statement
Stage 9N.4U discovered that while `IPluginCapability` encapsulates capability behavior and validation generation via `ArtifactGenerator`, it does not expose the generated source code payload to the Factory (`SaaSFoundry.EngineeringWorkbench.Builder`). `GenerateArtifactsAsync` returns `Task` (void equivalent). 

Consequently, the Factory cannot materialize the generated files into the `Generated/` directories. Since `IPluginCapability`, `IPluginExecutionContext`, and `SaaSFoundry.SDK.Core` are strictly frozen, a Factory-owned artifact staging directory mechanism must be used to extract generated artifacts deterministically without resorting to runtime assembly scanning.

## 2. Extraction Boundary Resolution
The extraction boundary utilizes **Factory-Owned Artifact Staging followed by Deterministic Import** (Mechanism E).

### Architecture
- **Plugin Execution (`SaaSFoundry.EngineeringWorkbench.PluginRuntime.Execution`)**: Remains unchanged. `PluginExecutionEngine` still invokes `GenerateArtifactsAsync`.
- **ExecutionContext (`IPluginExecutionContext`)**: The Factory passes a deterministic staging path via the context's `Arguments` array, e.g., `--extraction-path=.staging/{CellId}/{CapabilityId}/extraction.json`.
- **Plugin Capability Implementation**: Retrieves the staging path from `Arguments`. It invokes the frozen `ArtifactGenerator`, serializes the output (`result.GeneratedArtifacts`), and writes it to the extraction JSON file. This is the *only* physical I/O performed directly by the plugin.
- **Runtime Materializer (`SaaSFoundry.EngineeringWorkbench.Infrastructure.Host.RuntimeMaterializer`)**: A new Factory-owned service that coordinates code generation. It:
  1. Issues the plugin execution with the staging path.
  2. Deserializes the extracted artifact metadata and content from the `extraction.json` payload.
  3. Validates the extracted artifacts via `ValidationEngine`.
  4. Writes the artifacts into the authoritative `Generated/` target path using `ArtifactWriter`.

### Boundary Characteristics
1. **No Core/SDK Modification**: Uses the existing `Arguments` array to pass extraction coordinates.
2. **Deterministic Placement**: The `RuntimeMaterializer` strictly defines the target cell layout and verifies no path traversal occurs before writing the final payload to the `Generated/` directory.
3. **No Reflection**: Content is extracted securely and explicitly via JSON exchange rather than assembly scanning.
4. **Authoritative Ownership**: The Factory maintains exclusive control over final artifact placement, layout rules, and validation gating.

## 3. Status
**CERTIFIED**: The extraction mechanism satisfies the strict NativeAOT and determinism constraints of the VibeStock architecture while ensuring absolute Factory ownership of the materialization boundary.
