namespace SaaSFoundry.EngineeringWorkbench.Builder.Services;

public sealed class BuilderApplication
{
    private readonly CatalogLoader _catalogLoader;
    private readonly DocumentResolver _resolver;
    private readonly PluginGenerationService _generator;


    public BuilderApplication()
    {
        _catalogLoader = new CatalogLoader();

        _resolver = new DocumentResolver();

        _generator = new PluginGenerationService();
    }


    public async Task RunAsync(string[] args)
    {
        var root =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "plugins",
                "Observability");


        var catalog =
            await _catalogLoader.LoadAsync(
                Path.Combine(
                    root,
                    "catalog",
                    "observability.catalog.json"));


        Console.WriteLine(
            $"Generating plugin: {catalog.Plugin}");


        foreach(var document in catalog.Documents)
        {
            var artifact =
                _resolver.Resolve(document);


            await _generator.GenerateAsync(
                root,
                artifact);
        }


        Console.WriteLine(
            "Generation completed");
    }
}
