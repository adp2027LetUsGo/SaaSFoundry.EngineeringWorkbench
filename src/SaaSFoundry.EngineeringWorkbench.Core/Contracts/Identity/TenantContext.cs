namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;

/// <summary>
/// Represents the explicit tenant boundary for data isolation (the "Where").
/// Architecture Freeze v1.0.1
/// </summary>
public record TenantContext(
    string TenantId);
