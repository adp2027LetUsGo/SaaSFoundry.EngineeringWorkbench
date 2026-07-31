namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

public interface IPluginCapability
{
    string Id { get; }
    string Description { get; }
    System.Collections.Generic.IReadOnlyList<string> SupportedOperations { get; }
}
