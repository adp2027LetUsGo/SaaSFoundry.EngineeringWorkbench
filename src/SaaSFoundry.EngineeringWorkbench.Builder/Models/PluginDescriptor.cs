namespace SaaSFoundry.EngineeringWorkbench.Builder.Models;

public sealed class PluginDescriptor
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public List<PluginArtifact> Artifacts { get; set; } = [];
}
