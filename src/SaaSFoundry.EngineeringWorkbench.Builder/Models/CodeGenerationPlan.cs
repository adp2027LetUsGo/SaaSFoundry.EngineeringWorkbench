namespace SaaSFoundry.EngineeringWorkbench.Builder.Models;

public sealed class CodeGenerationPlan
{
    public string ProductId { get; set; } = string.Empty;
    public ProductDefinition Product { get; set; } = new();
    public List<CellGenerationPlan> Cells { get; set; } = [];
}

public sealed class CellGenerationPlan
{
    public string CellId { get; set; } = string.Empty;
    public string TargetPath { get; set; } = string.Empty;
    public List<CapabilityRegistrationMetadata> Registrations { get; set; } = [];
}
