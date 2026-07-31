namespace SaaSFoundry.EngineeringWorkbench.Builder.Services;

public sealed class TemplateEngine
{
    private readonly string _basePath;


    public TemplateEngine()
    {
        _basePath =
            Directory.GetCurrentDirectory();

        if (!Directory.Exists(
            Path.Combine(
                _basePath,
                "src")))
        {
            _basePath =
                Path.Combine(
                    _basePath,
                    "src",
                    "SaaSFoundry.EngineeringWorkbench.Builder");
        }
    }


    public async Task<string> RenderAsync(
        string templatePath,
        Dictionary<string,string> values)
    {
        var fullPath =
            Path.Combine(
                _basePath,
                templatePath);


        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                $"Template not found: {fullPath}");
        }


        var content =
            await File.ReadAllTextAsync(fullPath);


        foreach(var item in values)
        {
            content =
                content.Replace(
                    "{{" + item.Key + "}}",
                    item.Value);
        }


        return content;
    }
}
