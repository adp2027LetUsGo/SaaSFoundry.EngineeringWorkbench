namespace SaaSFoundry.EngineeringWorkbench.Builder.Services;

public sealed class TemplateResolver
{
    public string Resolve(
        string pluginRoot,
        string template)
    {
        return Path.Combine(
            pluginRoot,
            template);
    }
}
