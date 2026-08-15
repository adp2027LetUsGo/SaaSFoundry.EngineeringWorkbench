# Architecture Freeze v1.1.5

**OWNER:**
Factory / Builder

**PURPOSE:**
Provide deterministic runtime registration metadata for Host Generator composition without modifying frozen Core/plugin contracts.

**CORE:**
Unchanged

**SDK:**
Unchanged

**IPluginCapability:**
Unchanged

**REFLECTION:**
Forbidden

**NATIVE AOT:**
Compatible

**PRODUCT SPECIFIC LOGIC:**
Forbidden in Factory

**DETERMINISM:**
Mandatory

## Detail

The Factory currently cannot deterministically compose plugin-generated runtime registration fragments because `IPluginCapability` does not expose registration namespaces, extension method names, or deterministic composition metadata. Modifying `IPluginCapability` is forbidden under the frozen architecture.

This amendment formally introduces a Factory-owned configuration mechanism (e.g., `CapabilityRegistrationMetadata` within `PluginDescriptor`) that allows plugins to statically declare their C# runtime registration metadata (Namespace and Extension Method). The `CodeGenerationPlanner` reads this metadata to populate the `CodeGenerationPlan`, which the `HostGenerator` then uses to emit deterministic `Program.cs` compositions.
