using SaaSFoundry.EngineeringWorkbench.Builder.Models;

namespace SaaSFoundry.EngineeringWorkbench.Builder.Services;

public sealed class PluginGenerationService
{
    private readonly TemplateResolver _resolver;
    private readonly TemplateEngine _engine;
    private readonly ArtifactWriter _writer;


    public PluginGenerationService()
    {
        _resolver = new TemplateResolver();
        _engine = new TemplateEngine();
        _writer = new ArtifactWriter();
    }


    public async Task GenerateAsync(
        string pluginRoot,
        PluginArtifact artifact)
    {
        var template =
            _resolver.Resolve(
                pluginRoot,
                artifact.TemplatePath);


        var content =
            await _engine.RenderAsync(
                template,
                artifact.Metadata);


        await _writer.WriteAsync(
            artifact.OutputPath,
            content);


        Console.WriteLine(
            $"Generated {artifact.Id}");
    }
}
