using SaaSFoundry.EngineeringWorkbench.Builder.Models;

namespace SaaSFoundry.EngineeringWorkbench.Builder.Services;

public sealed class DocumentResolver
{
    public PluginArtifact Resolve(
        CatalogDocument document)
    {
        var template =
            document.Id.StartsWith("OBS-1")
                ? $"templates/implementation/{document.Id}.md.template"
                : $"templates/canonical/{document.Id}.md.template";


        return new PluginArtifact
        {
            Id = document.Id,

            TemplatePath = template,

            OutputPath =
                $"output/{document.Id}.md",

            Metadata =
            {
                ["Id"] = document.Id,
                ["Title"] = document.Title,
                ["Category"] = document.Category,
                ["Version"] = "1.0"
            }
        };
    }
}
