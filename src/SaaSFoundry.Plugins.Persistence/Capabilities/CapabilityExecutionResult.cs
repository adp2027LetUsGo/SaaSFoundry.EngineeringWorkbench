using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

namespace SaaSFoundry.Plugins.Persistence.Capabilities;

public readonly struct CapabilityExecutionResult : IPluginExecutionResult
{
    public int StatusCode { get; }

    public CapabilityExecutionResult(int statusCode)
    {
        StatusCode = statusCode;
    }
}
