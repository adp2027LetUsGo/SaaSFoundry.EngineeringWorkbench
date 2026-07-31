namespace SaaSFoundry.EngineeringWorkbench.Builder.Models;

public sealed class GenerationManifest
{
    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public List<ManifestArtifactDefinition> Artifacts { get; set; } = [];
}
