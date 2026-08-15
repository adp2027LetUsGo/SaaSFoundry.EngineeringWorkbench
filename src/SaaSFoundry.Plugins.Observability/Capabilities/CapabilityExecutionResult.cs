using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

namespace SaaSFoundry.Plugins.Observability.Capabilities;

public sealed record CapabilityExecutionResult(int StatusCode = 0) : IPluginExecutionResult;
