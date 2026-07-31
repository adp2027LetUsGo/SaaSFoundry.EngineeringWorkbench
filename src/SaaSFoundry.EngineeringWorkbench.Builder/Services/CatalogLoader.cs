using System.Text.Json;
using SaaSFoundry.EngineeringWorkbench.Builder.Models;

namespace SaaSFoundry.EngineeringWorkbench.Builder.Services;

public sealed class CatalogLoader
{
    public async Task<ObservabilityCatalog> LoadAsync(
        string path)
    {
        var json =
            await File.ReadAllTextAsync(path);


        return JsonSerializer.Deserialize<ObservabilityCatalog>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
            ?? throw new InvalidOperationException(
                "Invalid catalog");
    }
}
