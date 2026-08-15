# Architecture Freeze v1.1.6

**OWNER:**
Factory / Builder

**PURPOSE:**
Formalize the HostGenerator architecture to deterministically synthesize `Program.cs` for each Generated Product Runtime Cell based on the `CodeGenerationPlan`.

**CORE:**
Unchanged

**SDK:**
Unchanged

**IPluginCapability:**
Unchanged

**REFLECTION:**
Forbidden in generated Host. Must use static method calls for Native AOT compatibility.

**NATIVE AOT:**
Mandatory for generated Host.

**PRODUCT SPECIFIC LOGIC:**
Forbidden in Factory. The Factory must rely purely on `CodeGenerationPlan` metadata.

**DETERMINISM:**
Mandatory. Namespaces and extension methods must be alphabetically ordered by registration order and capability ID.

## Detail

The `HostGenerator` is the sole Factory-owned component responsible for synthesizing the entry point (`Program.cs`) of Generated Product Runtime Cells. It consumes a `CodeGenerationPlan`, which contains deterministically ordered capability registrations mapped to their required C# namespaces and extension methods.

The generator iterates over the target Cells defined in the plan, synthesizes the C# source code using Native AOT compatible `WebApplication.CreateBuilder(args)` patterns, and writes the `Program.cs` file into the target Cell's `<targetPath>/Generated` directory.

The `HostGenerator` does not use reflection, nor does it contain any product-specific logic or knowledge of specific capabilities. It acts purely as a deterministic materializer of the plan.
