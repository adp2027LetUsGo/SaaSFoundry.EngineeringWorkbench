using SaaSFoundry.EngineeringWorkbench.Builder.Models;

namespace SaaSFoundry.EngineeringWorkbench.Builder.Services;

public sealed class GenerationEngine
{
    private readonly TemplateEngine _templateEngine;
    private readonly ArtifactWriter _writer;


    public GenerationEngine()
    {
        _templateEngine = new TemplateEngine();
        _writer = new ArtifactWriter();
    }


    public async Task GenerateAsync(
        ManifestArtifactDefinition artifact)
    {
        Console.WriteLine(
            $"Generating {artifact.Id}");


        var content =
            await _templateEngine.RenderAsync(
                artifact.TemplatePath,
                artifact.Metadata);


        await _writer.WriteAsync(
            artifact.OutputPath,
            content);


        Console.WriteLine(
            $"Created {artifact.OutputPath}");
    }


    public async Task GenerateAllAsync(
        GenerationManifest manifest)
    {
        foreach(var artifact in manifest.Artifacts)
        {
            await GenerateAsync(artifact);
        }
    }
}
