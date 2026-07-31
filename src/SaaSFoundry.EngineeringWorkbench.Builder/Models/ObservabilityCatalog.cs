namespace SaaSFoundry.EngineeringWorkbench.Builder.Models;

public sealed class ObservabilityCatalog
{
    public string Plugin { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public List<CatalogDocument> Documents { get; set; } = [];
}
