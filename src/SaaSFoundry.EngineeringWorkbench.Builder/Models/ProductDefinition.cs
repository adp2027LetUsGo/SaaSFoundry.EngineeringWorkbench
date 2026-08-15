namespace SaaSFoundry.EngineeringWorkbench.Builder.Models;

public sealed class ProductDefinition
{
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<CellDefinition> Cells { get; set; } = [];
    public List<CommunicationEdge> Communications { get; set; } = [];
}

public sealed class CellDefinition
{
    public string CellId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TargetPath { get; set; } = string.Empty;
    public List<string> Capabilities { get; set; } = [];
}

public sealed class CommunicationEdge
{
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Mode { get; set; } = "Outbound";
}
