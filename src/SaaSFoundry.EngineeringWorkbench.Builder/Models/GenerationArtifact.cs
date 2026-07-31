namespace SaaSFoundry.EngineeringWorkbench.Builder.Models;

public sealed class GenerationArtifact
{
    public string Id { get; set; } = string.Empty;


    public string Title { get; set; } = string.Empty;


    public string TemplatePath { get; set; } = string.Empty;


    public string OutputPath { get; set; } = string.Empty;


    public Dictionary<string, string> Metadata { get; set; } = [];
}