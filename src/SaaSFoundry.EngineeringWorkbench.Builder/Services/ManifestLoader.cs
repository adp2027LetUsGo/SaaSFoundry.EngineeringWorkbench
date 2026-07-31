using System.Text.Json;
using SaaSFoundry.EngineeringWorkbench.Builder.Models;

namespace SaaSFoundry.EngineeringWorkbench.Builder.Services;

public sealed class ManifestLoader
{
    public async Task<GenerationManifest> LoadAsync(
        string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(path);
        }


        var json =
            await File.ReadAllTextAsync(path);


        var manifest =
            JsonSerializer.Deserialize<GenerationManifest>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


        return manifest
            ?? throw new InvalidOperationException(
                "Invalid manifest");
    }
}
