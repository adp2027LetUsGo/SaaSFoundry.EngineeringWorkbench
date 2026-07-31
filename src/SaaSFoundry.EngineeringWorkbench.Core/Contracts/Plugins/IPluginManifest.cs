namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

public interface IPluginManifest
{
    string Id { get; }
    string Name { get; }
    string Version { get; }
    string Description { get; }
    System.Collections.Generic.IReadOnlyList<string> Dependencies { get; }
    System.Collections.Generic.IReadOnlyList<string> Compatibility { get; }
}
