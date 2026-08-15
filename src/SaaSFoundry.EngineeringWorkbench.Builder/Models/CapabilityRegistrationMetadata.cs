namespace SaaSFoundry.EngineeringWorkbench.Builder.Models;

public sealed class CapabilityRegistrationMetadata
{
    public string CapabilityId { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string ExtensionMethod { get; set; } = string.Empty;
    public int RegistrationOrder { get; set; }
}
