using System.Text.Json;
using SaaSFoundry.EngineeringWorkbench.Builder.Models;

namespace SaaSFoundry.EngineeringWorkbench.Builder.Services;

public sealed class PluginLoader
{
    public async Task<PluginDescriptor> LoadAsync(
        string path)
    {
        var json =
            await File.ReadAllTextAsync(path);


        return JsonSerializer.Deserialize<PluginDescriptor>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
            ?? throw new InvalidOperationException(
                "Invalid plugin manifest");
    }
}
