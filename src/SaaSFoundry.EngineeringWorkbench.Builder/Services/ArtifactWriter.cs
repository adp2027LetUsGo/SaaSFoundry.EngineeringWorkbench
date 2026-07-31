namespace SaaSFoundry.EngineeringWorkbench.Builder.Services;

public sealed class ArtifactWriter
{
    public async Task WriteAsync(
        string output,
        string content)
    {
        var folder =
            Path.GetDirectoryName(output);

        if (!string.IsNullOrWhiteSpace(folder))
        {
            Directory.CreateDirectory(folder);
        }

        await File.WriteAllTextAsync(
            output,
            content);
    }
}
